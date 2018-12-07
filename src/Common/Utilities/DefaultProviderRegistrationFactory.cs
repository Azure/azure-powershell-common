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

using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.Common.Utilities
{
    /// <summary>
    /// A generic provider registration factory
    /// </summary>
    public class DefaultProviderRegistrationFactory : IProviderRegistrationFactory
    {
        /// <summary>
        /// The key used to retireve the provider registration factory
        /// </summary>
        public const string ProviderRegistrationFactoryName = nameof(DefaultProviderRegistrationFactory);

        public IProviderRegistrar GetRegistrar(IAuthenticationFactory factory, IAzureContext context, Action<string> messageWriter)
        {
            return new NullProviderRegistrar();
        }

        class NullProviderRegistrar : IProviderRegistrar
        {
            public object Clone()
            {
                return new NullProviderRegistrar();
            }

            public string GetProviderNamespace(Uri requestUri)
            {
                return string.Empty;
            }

            public bool IsNotRegisteredError(HttpResponseMessage message)
            {
                return true;
            }

            public bool IsRegistered(string providerName)
            {
                return true;
            }

            public Task<bool> Register(string providerName)
            {
                return Task.FromResult(true);
            }
        }
    }
}
