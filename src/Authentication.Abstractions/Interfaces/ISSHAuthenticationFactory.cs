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
using System.Security;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// Factory for creating authentication artifacts for SSH connections
    /// </summary>
    public interface ISSHAuthenticationFactory
    {
        /// <summary>
        /// Authenticates an account for SSH connections
        /// </summary>
        /// <param name="sshParameters">The SSH key parameters that will be associated with the authenticated account</param>
        /// <param name="account">The azure account object</param>
        /// <param name="environment">The azure environment object</param>
        /// <param name="tenant">The AD tenant (in most cases will be 'common')</param>
        /// <param name="password">The AD account password</param>
        /// <param name="promptAction">The prompt action used in DeviceFlow authentication</param>
        /// <param name="tokenCache">The token cache</param>
        /// <param name="resourceId">Optional, the AD resource ID</param>
        /// <returns>An <see cref="IAccessToken"/> where the <see cref="IAccessToken.AccessToken"/> is populated with the SSH certificate.</returns>
        IAccessToken Authenticate(
            ISSHCertificateAuthenticationParameters sshParameters,
            IAzureAccount account,
            IAzureEnvironment environment,
            string tenant,
            SecureString password,
            Action<string> promptAction,
            IAzureTokenCache tokenCache,
            string resourceId = AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId);

        /// <summary>
        /// Authenticates an account for SSH connections
        /// </summary>
        /// <param name="sshParameters">The SSH key parameters that will be associated with the authenticated account</param>
        /// <param name="account">The azure account object</param>
        /// <param name="environment">The azure environment object</param>
        /// <param name="tenant">The AD tenant (in most cases will be 'common')</param>
        /// <param name="password">The AD account password</param>
        /// <param name="promptAction">The prompt action used in DeviceFlow authentication</param>
        /// <param name="resourceId">Optional, the AD resource ID</param>
        /// <returns>An <see cref="IAccessToken"/> where the <see cref="IAccessToken.AccessToken"/> is populated with the SSH certificate.</returns>
        IAccessToken Authenticate(
            ISSHCertificateAuthenticationParameters sshParameters,
            IAzureAccount account,
            IAzureEnvironment environment,
            string tenant,
            SecureString password,
            Action<string> promptAction,
            string resourceId = AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId);

        /// <summary>
        /// Gets SSH client credentials for the given context
        /// </summary>
        /// <param name="sshParams">The SSH key parameters that will be associated with the authenticated account</param>
        /// <param name="context">The target azure context</param>
        /// <returns>Credentials for the given context that can be used by SSH clients</returns>
        ISSHClientCertificateCredentials GetClientCertificateCredentials(
            ISSHCertificateAuthenticationParameters sshParams,
            IAzureContext context);
    }
}
