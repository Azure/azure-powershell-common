// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.Common.Authentication.Models
{
    /// <summary>
    /// Delegating Handlere that catches registration errors and attempts to register the provider
    /// </summary>
    public class RPRegistrationDelegatingHandler : DelegatingHandler, ICloneable
    {
        private const short RetryCount = 3;
        private Func<IProviderRegistrar> _getRegistrar;
        private Action<string> _logger;

        public RPRegistrationDelegatingHandler(Func<IProviderRegistrar> getRegistrar, Action<string> logger)
        {
            _getRegistrar = getRegistrar;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var registrar = _getRegistrar();
            if (registrar.IsNotRegisteredError(responseMessage))
            {
                var providerName = registrar.GetProviderNamespace(request.RequestUri);
                if (!string.IsNullOrEmpty(providerName) && !registrar.IsRegistered(providerName))
                {
                    bool isRegistered;
                    // Force dispose for response messages to reclaim the used socket connection.
                    responseMessage.Dispose();
                    try
                    {
                        short retryCount = 0;
                        do
                        {
                            if (retryCount++ > RetryCount)
                            {
                                throw new TimeoutException();
                            }
                            
                            isRegistered = await registrar.Register(providerName);
                            TestMockSupport.Delay(1000);
                        } while (!isRegistered);
                        _logger(string.Format(Resources.ResourceProviderRegisterSuccessful, providerName));
                    }
                    catch (Exception e)
                    {
                        _logger(string.Format(Resources.ResourceProviderRegisterFailure, providerName, e.Message));
                        // Ignore RP registration errors.
                    }

                    responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                }
            }

            return responseMessage;
        }

        public object Clone()
        {
            return new RPRegistrationDelegatingHandler(_getRegistrar, _logger);
        }
    }
}
