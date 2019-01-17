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
using Microsoft.Azure.Commands.Common.Authentication.Properties;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Microsoft.Azure.Commands.Common.Authentication
{
    public class ManagedServiceAccessTokenWebAppsMsiAdHoc : IRenewableToken
    {
        private readonly IAzureAccount _account;
        private const string ResourceId = @"https://management.azure.com/";
        private readonly IHttpOperations<ManagedServiceTokenInfoWebAppsMsiAdHoc> _tokenGetter;
        private DateTime _expiration = DateTime.UtcNow;
        private string _accessToken;

        public ManagedServiceAccessTokenWebAppsMsiAdHoc(IAzureAccount account, IAzureEnvironment environment, string tenant = "Common")
        {
            if (account == null || string.IsNullOrEmpty(account.Id) || !account.IsPropertySet(AzureAccount.Property.MSILoginUri))
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            TenantId = tenant;
            _account = account;

            if (!AzureSession.Instance.TryGetComponent(HttpClientOperationsFactory.Name, out IHttpOperationsFactory factory))
            {
                factory = HttpClientOperationsFactory.Create();
            }

            var baseUri = _account.GetProperty(AzureAccount.Property.MSILoginUri);
            var uri = $"{baseUri}?resource={ResourceId}&api-version=2017-09-01";
            RequestUris.Enqueue(uri);
            _tokenGetter = factory
                .GetHttpOperations<ManagedServiceTokenInfoWebAppsMsiAdHoc>(true)
                .WithHeader("Metadata", new[] { "true" })
                .WithHeader("Secret", new[] { account.GetProperty(AzureAccount.Property.MSILoginSecret) });
        }

        public string AccessToken
        {
            get
            {
                try
                {
                    GetOrRenewAuthentication();
                }
                catch (CloudException httpException)
                {
                    throw new InvalidOperationException(string.Format(Resources.MSITokenRequestFailed, ResourceId, httpException?.Request?.RequestUri?.ToString()), httpException);
                }

                return _accessToken;
            }
        }

        public Queue<string> RequestUris { get; } = new Queue<string>();

        public string LoginType => "ManagedService";

        public string TenantId { get; }

        public string UserId => _account.Id;

        public DateTimeOffset ExpiresOn => _expiration;

        public void AuthorizeRequest(Action<string, string> authTokenSetter)
        {
            authTokenSetter("Bearer", AccessToken);
        }

        private void GetOrRenewAuthentication()
        {
            if (_expiration - DateTime.UtcNow >= ManagedServiceTokenInfo.TimeoutThreshold) return;
            ManagedServiceTokenInfoWebAppsMsiAdHoc infoWebApps = null;
            while (infoWebApps == null && RequestUris.Count > 0)
            {
                var currentRequestUri = RequestUris.Dequeue();
                try
                {
                    infoWebApps = _tokenGetter.GetAsync(currentRequestUri, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                    // if a request was succesful, we should not check any other Uris
                    RequestUris.Clear();
                    RequestUris.Enqueue(currentRequestUri);
                }
                catch (Exception e) when ( (e is CloudException || e is HttpRequestException) && RequestUris.Count > 0)
                {
                    // skip to the next uri
                }
            }

            SetToken(infoWebApps);
        }

        private void SetToken(ManagedServiceTokenInfoWebAppsMsiAdHoc infoWebApps)
        {
            if (infoWebApps != null)
            {
                _expiration = infoWebApps.ExpiresOn;
                _accessToken = infoWebApps.AccessToken;
            }
        }

    }
}
