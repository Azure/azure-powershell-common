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

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// A generic registrar for ARM resources, indpendent of the ARM API version
    /// </summary>
    public interface IProviderRegistrar: ICloneable
    {
        /// <summary>
        /// Determine whether a provider has been previously registered
        /// </summary>
        /// <param name="providerNamespace">The namespace of the provider in ARM</param>
        /// <returns>true if previously registered, otherwise false</returns>
        bool IsRegistered(string providerNamespace);

        /// <summary>
        /// Determine if the http response message is a provider not registered error
        /// </summary>
        /// <param name="message">The message for an unsuccesssful request</param>
        /// <returns>true if the error indicates the provider is not registered, false otherwise</returns>
        bool IsNotRegisteredError(HttpResponseMessage message);

        /// <summary>
        /// Returns the provider namespace for the given request Uri
        /// </summary>
        /// <param name="RequestUri">The request URI containing the needed provider namespace</param>
        /// <returns>The provider namespace from the request</returns>
        string GetProviderNamespace(Uri requestUri);

        /// <summary>
        /// Attempts to register a provider once
        /// </summary>
        /// <param name="providerNamespace"></param>
        /// <returns>True if the registration state from the provider indicates successful registration, false otherwise</returns>
        Task<bool> Register(string providerNamespace);
    }
}
