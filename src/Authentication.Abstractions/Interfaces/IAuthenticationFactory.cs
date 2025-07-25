﻿// ----------------------------------------------------------------------------------
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

using Microsoft.Rest;
using System;
using System.Security;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// The factory for creating authentication artifcats for http, hyak, and autorest clients
    /// </summary>
    public interface IAuthenticationFactory : IHyakAuthenticationFactory
    {

        /// <summary>
        /// Returns IAccessToken if authentication succeeds or throws an exception if authentication fails.
        /// </summary>
        /// <param name="account">The azure account object</param>
        /// <param name="environment">The azure environment object</param>
        /// <param name="tenant">The AD tenant in most cases should be 'common'</param>
        /// <param name="password">The AD account password</param>
        /// <param name="promptBehavior">The prompt behavior</param>
        /// <param name="promptAction">The prompt action used in DeviceFlow authentication</param>
        /// <param name="optionalParameters">The optional parameters, may include TokenCache, ResourceId and CmdletContext</param>
        /// <returns></returns>
        IAccessToken Authenticate(
            IAzureAccount account,
            IAzureEnvironment environment,
            string tenant,
            SecureString password,
            string promptBehavior,
            Action<string> promptAction,
            IDictionary<string, object> optionalParameters);

        /// <summary>
        /// Returns IAccessToken if authentication succeeds or throws an exception if authentication fails.
        /// </summary>
        /// <param name="account">The azure account object</param>
        /// <param name="environment">The azure environment object</param>
        /// <param name="tenant">The AD tenant in most cases should be 'common'</param>
        /// <param name="password">The AD account password</param>
        /// <param name="promptBehavior">The prompt behavior</param>
        /// <param name="promptAction">The prompt action used in DeviceFlow authentication</param>
        /// <param name="tokenCache">Token Cache</param>
        /// <param name="resourceId">Optional, the AD resource id</param>
        /// <returns></returns>
        IAccessToken Authenticate(
            IAzureAccount account,
            IAzureEnvironment environment,
            string tenant,
            SecureString password,
            string promptBehavior,
            Action<string> promptAction,
            IAzureTokenCache tokenCache,
            string resourceId = AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId);

        /// <summary>
        /// Returns IAccessToken if authentication succeeds or throws an exception if authentication fails.
        /// </summary>
        /// <param name="account">The azure account object</param>
        /// <param name="environment">The azure environment object</param>
        /// <param name="tenant">The AD tenant in most cases should be 'common'</param>
        /// <param name="password">The AD account password</param>
        /// <param name="promptBehavior">The prompt behavior</param>
        /// <param name="promptAction">The prompt action used in DeviceFlow authentication</param>
        /// <param name="resourceId">Optional, the AD resource id</param>
        /// <returns></returns>
        IAccessToken Authenticate(
            IAzureAccount account,
            IAzureEnvironment environment,
            string tenant,
            SecureString password,
            string promptBehavior,
            Action<string> promptAction,
            string resourceId = AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId);

        /// <summary>
        /// Get AutoRest credentials for the given context
        /// </summary>
        /// <param name="context">The target azure context</param>
        /// <returns>AutoRest client credentials targeting the given context</returns>
        ServiceClientCredentials GetServiceClientCredentials(IAzureContext context);

        /// <summary>
        /// Get AutoRest credentials for the given context
        /// </summary>
        /// <param name="context">The target azure context</param>
        /// <param name="cmdletContext">The caller cmdlet context</param>
        /// <returns>AutoRest client credentials targeting the given context</returns>
        ServiceClientCredentials GetServiceClientCredentials(IAzureContext context, ICmdletContext cmdletContext);

        /// <summary>
        /// Get AutoRest credebntials using the given context and named endpoint
        /// </summary>
        /// <param name="context">The context to use for authentication</param>
        /// <param name="targetEndpoint">The named endpoint the AutoRest client will target</param>
        /// <returns>AutoRest client crentials targeting the given context and endpoint</returns>
        ServiceClientCredentials GetServiceClientCredentials(IAzureContext context, string targetEndpoint);

        /// <summary>
        /// Get AutoRest credentials using the given context and named endpoint
        /// </summary>
        /// <param name="context">The context to use for authentication</param>
        /// <param name="targetEndpoint">The named endpoint the AutoRest client will target</param>
        /// <param name="cmdletContext">The caller cmdlet context</param>
        /// <returns>AutoRest client credentials targeting the given context and endpoint</returns>
        ServiceClientCredentials GetServiceClientCredentials(IAzureContext context, string targetEndpoint, ICmdletContext cmdletContext);

        /// <summary>
        /// Get service client credentials with initial token and delegate for renewing
        /// </summary>
        /// <param name="accessToken">Initial token for credential</param>
        /// <param name="renew"></param>
        /// <returns>Service client credentials</returns>
        ServiceClientCredentials GetServiceClientCredentials(string accessToken, Func<string> renew = null);

        /// <summary>
        /// Remove any stored credentials for the given user
        /// </summary>
        /// <param name="account">The account to remove credentials for</param>
        /// <param name="tokenCache">The TokenCache to remove credentials from</param>
        [Obsolete("RemoveUser is deprecated, please use RemoveUser with Azure environment instead.", true)]
        void RemoveUser(IAzureAccount account, IAzureTokenCache tokenCache);


        /// <summary>
        /// Remove any stored credentials for the given user and the Azure environment used.
        /// </summary>
        /// <param name="account">The account to remove credentials for</param>
        /// <param name="environment">The environment which account belongs to</param>
        void RemoveUser(IAzureAccount account, IAzureEnvironment environment);
    }
}
