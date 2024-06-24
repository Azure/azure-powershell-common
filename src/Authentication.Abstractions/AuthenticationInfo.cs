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

using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    public class AuthenticationInfo : IAuthenticationInfo
    {
        /// <summary>
        /// Class name of the TokenCredential, stands for the authentication method
        /// </summary>
        public string TokenCredentialName { get; set; }

        /// <summary>
        /// Authentication process succeed or not.
        /// </summary>
        public bool AuthenticationSuccess { get; set; } = false;

        /// <summary>
        /// Additional properties for AuthenticationInfo
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        public IDictionary<string, string> ExtendedProperties { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public AuthenticationInfo()
        {
            TokenCredentialName = null;
        }

        public AuthenticationInfo(IAuthenticationInfo other, bool? isSuccess = null)
        {
            this.TokenCredentialName = other.TokenCredentialName;
            this.AuthenticationSuccess = isSuccess ?? other.AuthenticationSuccess;
            foreach(var property in other.ExtendedProperties)
            {
                this.SetProperty(property.Key, property.Value);
            }
        }

        /// <summary>
        /// Key to show whether token cache is enabled or not.
        /// </summary>
        public const string TokenCacheEnabled = "TokenCacheEnabled";

        public const string AuthInfoTelemetryHeadKey = "auth-info-head";

        public const string AuthInfoTelemetrySubsequentKey = "auth-info-sub";
    }
}
