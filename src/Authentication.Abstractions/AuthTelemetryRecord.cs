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

using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// Represents a telemetry record for authentication.
    /// </summary>
    public class AuthTelemetryRecord : IAuthTelemetryRecord
    {
        /// <summary>
        /// Gets or sets the class name of the TokenCredential, which stands for the authentication method.
        /// </summary>
        public string TokenCredentialName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the authentication process succeeded or not.
        /// </summary>
        public bool AuthenticationSuccess { get; set; } = false;

        /// <summary>
        /// Gets or sets the correlation ID for the authentication.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets the additional properties for AuthenticationInfo.
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, string> ExtendedProperties { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthTelemetryRecord"/> class.
        /// </summary>
        public AuthTelemetryRecord()
        {
            TokenCredentialName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthTelemetryRecord"/> class based on another instance of <see cref="IAuthTelemetryRecord"/>.
        /// </summary>
        /// <param name="other">The other instance of <see cref="IAuthTelemetryRecord"/>.</param>
        /// <param name="isSuccess">A value indicating whether the authentication was successful or not.</param>
        public AuthTelemetryRecord(IAuthTelemetryRecord other, bool? isSuccess = null)
        {
            this.TokenCredentialName = other.TokenCredentialName;
            this.AuthenticationSuccess = isSuccess ?? other.AuthenticationSuccess;
            foreach(var property in other.ExtendedProperties)
            {
                this.SetProperty(property.Key, property.Value);
            }
        }

        /// <summary>
        /// Represents the key to indicate whether the token cache is enabled or not.
        /// </summary>
        public const string TokenCacheEnabled = "TokenCacheEnabled";

        /// <summary>
        /// Represents the prefix of properties of the first record of authentication telemetry record.
        /// </summary>
        public const string AuthTelemetryPropertyHeadPrefix = "auth-info-head";

        /// <summary>
        /// Represents the key of the left records of authentication telemetry.
        /// </summary>
        public const string AuthTelemetryPropertyTailKey = "auth-info-tail";
    }
}
