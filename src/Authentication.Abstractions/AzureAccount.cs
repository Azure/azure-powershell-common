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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// a model class for azure accoutn credentials
    /// </summary>
    [Serializable]
    public class AzureAccount : IAzureAccount
    {
        /// <summary>
        /// The account displayable id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The account credentials
        /// </summary>
        public string Credential { get; set; }

        /// <summary>
        /// The accoutn and credential type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A record of the account identifier in each tenant the accoutn has access to
        /// </summary>
        public IDictionary<string, string> TenantMap { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Additional proeprties for accounts
        /// </summary>
        public IDictionary<string, string> ExtendedProperties { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// identifier specific equality comparer
        /// </summary>
        /// <param name="obj">Another account</param>
        /// <returns>true if accounts are equal, fase if the other object is a different account or a different object</returns>
        public override bool Equals(object obj)
        {
            var anotherAccount = obj as AzureAccount;
            if (anotherAccount == null)
            {
                return false;
            }
            else
            {
                return string.Equals(anotherAccount.Id, Id, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Ensures that accounts representing the same id use the same hash
        /// </summary>
        /// <returns>A hash value based on the account displayable id</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// string constants for known credential types
        /// </summary>
        public static class AccountType
        {
            public const string Certificate = "Certificate",
            User = "User",
            ServicePrincipal = "ServicePrincipal",
            AccessToken = "AccessToken",
            ManagedService = "ManagedService";
        }

        /// <summary>
        /// string constants for known extended properties
        /// </summary>
        public static class Property
        {
            public const string Subscriptions = "Subscriptions";

            /// <summary>
            /// Comma separated list of tenants on this account.
            /// </summary>
            public const string Tenants = "Tenants";

            /// <summary>
            /// Access token.
            /// </summary>
            public const string AccessToken = "AccessToken";

            /// <summary>
            /// Account object id + home tenant id.
            /// </summary>
            public const string HomeAccountId = "HomeAccountId";

            /// <summary>
            /// Indicate whether to use user name and password for authentication.
            /// </summary>
            public const string UsePasswordAuth = "UsePasswordAuth";

            /// <summary>
            /// Access token for AD Graph service.
            /// </summary>
            public const string GraphAccessToken = "GraphAccessToken";

            /// <summary>
            /// Access token for KeyVault service.
            /// </summary>
            public const string KeyVaultAccessToken = "KeyVault";

            /// <summary>
            /// Thumbprint for associated certificate
            /// </summary>
            public const string CertificateThumbprint = "CertificateThumbprint";

            /// <summary>
            /// Login Uri for Managed Service Login
            /// </summary>
            public const string MSILoginUri = "MSILoginUri";

            /// <summary>
            /// Backup login Uri for MSI
            /// </summary>
            public const string MSILoginUriBackup = "MSILoginBackup";

            /// <summary>
            /// Secret that may be used with MSI login
            /// </summary>
            public const string MSILoginSecret = "MSILoginSecret";

            /// <summary>
            /// Secret that may be used with service principal login
            /// </summary>
            public const string ServicePrincipalSecret = "ServicePrincipalSecret";


            /// <summary>
            /// The path of certficate file in pem or pkcs#12 format
            /// </summary>
            public const string CertificatePath = "CertificatePath";

            /// <summary>
            /// The password required to access the pkcs#12 certificate file
            /// </summary>
            public const string CertificatePassword = "CertificatePassword";


        /// <summary>
        /// Specifies if the x5c claim (public key of the certificate) should be sent to the STS to achieve easy certificate rollover in Azure AD
        /// </summary>
            public const string SendCertificateChain = "SendCertificateChain";
        }
    }
}
