// ----------------------------------------------------------------------------------
// Copyright Microsoft Corporation
//
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

using Hyak.Common;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
// TODO: Remove IfDef
#if NETSTANDARD
using Microsoft.WindowsAzure.Commands.Common;
#endif
using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.Azure.Commands.Common.Authentication.Properties;

namespace Microsoft.Azure.Commands.Common.Authentication
{
    internal class ServicePrincipalTokenProvider : ITokenProvider
    {
        private static readonly TimeSpan ExpirationThreshold = TimeSpan.FromMinutes(5);
        private readonly Func<IServicePrincipalKeyStore> _getKeyStore;
        private IServicePrincipalKeyStore _keyStore;

        public IServicePrincipalKeyStore KeyStore
        {
            get { return _keyStore ?? (_keyStore = _getKeyStore()); }
            set
            {
                _keyStore = value;
            }
        }

        public ServicePrincipalTokenProvider()
        {
        }

        public ServicePrincipalTokenProvider(Func<IServicePrincipalKeyStore> getKeyStore)
        {
            _getKeyStore = getKeyStore;
        }

        public IAccessToken GetAccessToken(
            AdalConfiguration config,
            string promptBehavior,
            Action<string> promptAction,
            string userId,
            SecureString password,
            string credentialType)
        {
            if (credentialType == AzureAccount.AccountType.User)
            {
                throw new ArgumentException(string.Format(Resources.InvalidCredentialType, "User"), nameof(credentialType));
            }
            return new ServicePrincipalAccessToken(config, AcquireTokenWithSecret(config, userId, password), RenewWithSecret, userId);
        }

        public IAccessToken GetAccessTokenWithCertificate(
            AdalConfiguration config,
            string clientId,
            string certificateThumbprint,
            string credentialType)
        {
            if (credentialType == AzureAccount.AccountType.User)
            {
                throw new ArgumentException(string.Format(Resources.InvalidCredentialType, "User"), nameof(credentialType));
            }
            return new ServicePrincipalAccessToken(
                config,
                AcquireTokenWithCertificate(config, clientId, certificateThumbprint),
                (adalConfig, appId) => RenewWithCertificate(adalConfig, appId, certificateThumbprint), clientId);
        }

        private static AuthenticationContext GetContext(AdalConfiguration config)
        {
            var authority = config.AdEndpoint + config.AdDomain;
            return new AuthenticationContext(authority, config.ValidateAuthority, config.TokenCache);
        }

        private AuthenticationResult AcquireTokenWithSecret(AdalConfiguration config, string appId, SecureString appKey)
        {
            if (appKey == null)
            {
                return RenewWithSecret(config, appId);
            }

            StoreAppKey(appId, config.AdDomain, appKey);
            var context = GetContext(config);
// TODO: Remove IfDef
#if NETSTANDARD
            var credential = new ClientCredential(appId, ConversionUtilities.SecureStringToString(appKey));
            return context.AcquireTokenAsync(config.ResourceClientUri, credential).ConfigureAwait(false).GetAwaiter().GetResult();
#else
            var credential = new ClientCredential(appId, appKey);
            return context.AcquireToken(config.ResourceClientUri, credential);
#endif
        }

        private static AuthenticationResult AcquireTokenWithCertificate(
            AdalConfiguration config,
            string appId,
            string thumbprint)
        {
            var certificate = AzureSession.Instance.DataStore.GetCertificate(thumbprint);
            if (certificate == null)
            {
                throw new ArgumentException(string.Format(Resources.CertificateNotFoundInStore, thumbprint));
            }

            var context = GetContext(config);
// TODO: Remove IfDef
#if NETSTANDARD
            return context.AcquireTokenAsync(config.ResourceClientUri, new ClientAssertionCertificate(appId, certificate))
                          .ConfigureAwait(false).GetAwaiter().GetResult();
#else
            return context.AcquireToken(config.ResourceClientUri, new ClientAssertionCertificate(appId, certificate));
#endif
        }

        private AuthenticationResult RenewWithSecret(AdalConfiguration config, string appId)
        {
            TracingAdapter.Information(Resources.SPNRenewTokenTrace, appId, config.AdDomain, config.AdEndpoint,
                config.ClientId, config.ClientRedirectUri);
// TODO: Remove IfDef
#if NETSTANDARD
            var appKey = LoadAppKey(appId, config.AdDomain);
            if (appKey == null)
            {
                throw new KeyNotFoundException(string.Format(Resources.ServiceKeyNotFound, appId));
            }
            return AcquireTokenWithSecret(config, appId, appKey);
#else
            using (SecureString appKey = LoadAppKey(appId, config.AdDomain))
            {
                if (appKey == null)
                {
                    throw new KeyNotFoundException(string.Format(Resources.ServiceKeyNotFound, appId));
                }
                return AcquireTokenWithSecret(config, appId, appKey);
            }
#endif
        }

        private static AuthenticationResult RenewWithCertificate(
            AdalConfiguration config,
            string appId,
            string thumbprint)
        {
            TracingAdapter.Information(
                Resources.SPNRenewTokenTrace,
                appId,
                config.AdDomain,
                config.AdEndpoint,
                config.ClientId,
                config.ClientRedirectUri);
            return AcquireTokenWithCertificate(config, appId, thumbprint);
        }

        private SecureString LoadAppKey(string appId, string tenantId)
        {
            return KeyStore.GetKey(appId, tenantId);
        }

        private void StoreAppKey(string appId, string tenantId, SecureString appKey)
        {
            KeyStore.SaveKey(appId, tenantId, appKey);
        }

        private class ServicePrincipalAccessToken : IRenewableToken
        {
            private readonly AdalConfiguration _configuration;
            private AuthenticationResult _authResult;
            private readonly Func<AdalConfiguration, string, AuthenticationResult> _tokenRenewer;

            public ServicePrincipalAccessToken(
                AdalConfiguration configuration,
                AuthenticationResult authResult,
                Func<AdalConfiguration, string, AuthenticationResult> tokenRenewer,
                string appId)
            {
                _configuration = configuration;
                _authResult = authResult;
                _tokenRenewer = tokenRenewer;
                UserId = appId;
            }

            public void AuthorizeRequest(Action<string, string> authTokenSetter)
            {
                if (IsExpired)
                {
                    _authResult = _tokenRenewer(_configuration, UserId);
                }

                authTokenSetter(_authResult.AccessTokenType, _authResult.AccessToken);
            }

            public string UserId { get; }

            public string AccessToken { get { return _authResult.AccessToken; } }

            public string LoginType { get { return Authentication.LoginType.OrgId; } }

            public string TenantId { get { return _configuration.AdDomain; } }

            private bool IsExpired
            {
                get
                {
#if DEBUG
                    if (Environment.GetEnvironmentVariable("FORCE_EXPIRED_ACCESS_TOKEN") != null)
                    {
                        return true;
                    }
#endif

                    var expiration = _authResult.ExpiresOn;
                    var currentTime = DateTimeOffset.UtcNow;
                    var timeUntilExpiration = expiration - currentTime;
                    TracingAdapter.Information(
                        Resources.SPNTokenExpirationCheckTrace,
                        expiration,
                        currentTime,
                        ExpirationThreshold,
                        timeUntilExpiration);
                    return timeUntilExpiration < ExpirationThreshold;
                }
            }

            public DateTimeOffset ExpiresOn { get { return _authResult.ExpiresOn; } }
        }
    }
}

