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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// A model for an Azure Active Directory Tenant
    /// </summary>
    [Serializable]
    public class AzureTenant : IAzureTenant
    {
        /// <summary>
        /// The tenant ID (globally-unique identifier)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The domain name suffix for the directory (domain)
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Whether tenant category is home
        /// </summary>
        public bool IsHome => string.IsNullOrEmpty(this.GetProperty(Property.TenantCategory))
                    || this.GetProperty(Property.TenantCategory).Equals("Home", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Custom proeprties of the tenant
        /// </summary>
        public IDictionary<string, string> ExtendedProperties { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static class Property
        {
            public const string TenantCategory = "TenantCategory",
                Country = "Country",
                CountryCode = "CountryCode",
                DisplayName = "DisplayName",
                Domains = "Domains",
                DefaultDomain = "DefaultDomain",
                TenantType = "TenantType",
                TenantBrandingLogoUrl = "TenantBrandingLogoUrl",
                Directory = "Directory";
        }
    }
}
