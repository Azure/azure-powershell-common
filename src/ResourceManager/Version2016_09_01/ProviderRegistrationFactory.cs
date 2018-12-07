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

using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Management.Internal.Resources;
using Microsoft.Azure.Management.Internal.Resources.Models;
using Microsoft.WindowsAzure.Commands.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.ResourceManager.Common.Version2016_09_01
{
    public class ProviderRegistrationFactory : IProviderRegistrationFactory
    {
        class ProviderRegistrar : IProviderRegistrar
        {
            /// <summary>
            /// Contains all providers we have attempted to register 
            /// </summary>
            private HashSet<string> _registeredProviders;

            private Func<ResourceManagementClient> _createClient;

            private Action<string> _writeDebug;

            private ResourceManagementClient Client => _createClient();

            public ProviderRegistrar(HashSet<string> providers, Func<ResourceManagementClient> clientCreator, Action<string> logger)
            {
                _registeredProviders = providers;
                _createClient = clientCreator;
                _writeDebug = logger;
            }

            public ProviderRegistrar(Func<ResourceManagementClient> clientCreator, Action<string> logger)
            {
                _registeredProviders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _createClient = clientCreator;
                _writeDebug = logger;
            }

            public object Clone()
            {
                return new ProviderRegistrar(_registeredProviders, _createClient, _writeDebug);
            }

            public bool IsRegistered(string providerName)
            {
                return _registeredProviders.Contains(providerName);
            }

            public async Task<bool> Register(string providerName)
            {
                _registeredProviders.Add(providerName);
                var result = await Client.Providers.RegisterAsync(providerName);
                return string.Equals(RegistrationState.Registered, result?.RegistrationState, StringComparison.OrdinalIgnoreCase);
            }

            public bool IsNotRegisteredError(HttpResponseMessage message)
            {
                return message.StatusCode == System.Net.HttpStatusCode.Conflict &&
                    message.Content.ReadAsStringAsync().Result.Contains("registered to use namespace");
            }

            public string GetProviderNamespace(Uri requestUri)
            {
                return (requestUri.Segments.Length > 7 && requestUri.Segments[5].ToLower() == "providers/") ?
                    requestUri.Segments[6].ToLower().Trim('/') : null;
            }
        }

        public IProviderRegistrar GetRegistrar(IAuthenticationFactory factory, IAzureContext context, Action<string> messageWriter)
        {
            // Create a client without the standard pipeline
            return new ProviderRegistrar(() =>
                {
                    var client = new ResourceManagementClient(
                    context?.Environment?.GetEndpointAsUri(AzureEnvironment.Endpoint.ResourceManager),
                    factory.GetServiceClientCredentials(context, AzureEnvironment.Endpoint.ResourceManager));
                    client.SubscriptionId = context.Subscription.Id;
                   return client;
                 }, messageWriter);
        }
    }
}
