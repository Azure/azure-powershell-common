// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Commands.Common.MSGraph.Version1_0.Identity.DirectoryManagement.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// licenseUnitsDetail
    /// </summary>
    public partial class MicrosoftgraphlicenseUnitsDetail
    {
        /// <summary>
        /// Initializes a new instance of the MicrosoftgraphlicenseUnitsDetail
        /// class.
        /// </summary>
        public MicrosoftgraphlicenseUnitsDetail()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MicrosoftgraphlicenseUnitsDetail
        /// class.
        /// </summary>
        /// <param name="additionalProperties">Unmatched properties from the
        /// message are deserialized this collection</param>
        /// <param name="enabled">The number of units that are enabled for the
        /// active subscription of the service SKU.</param>
        /// <param name="suspended">The number of units that are suspended
        /// because the subscription of the service SKU has been cancelled. The
        /// units cannot be assigned but can still be reactivated before they
        /// are deleted.</param>
        /// <param name="warning">The number of units that are in warning
        /// status. When the subscription of the service SKU has expired, the
        /// customer has a grace period to renew their subscription before it
        /// is cancelled (moved to a suspended state).</param>
        public MicrosoftgraphlicenseUnitsDetail(IDictionary<string, object> additionalProperties = default(IDictionary<string, object>), int? enabled = default(int?), int? suspended = default(int?), int? warning = default(int?))
        {
            AdditionalProperties = additionalProperties;
            Enabled = enabled;
            Suspended = suspended;
            Warning = warning;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets unmatched properties from the message are deserialized
        /// this collection
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the number of units that are enabled for the active
        /// subscription of the service SKU.
        /// </summary>
        [JsonProperty(PropertyName = "enabled")]
        public int? Enabled { get; set; }

        /// <summary>
        /// Gets or sets the number of units that are suspended because the
        /// subscription of the service SKU has been cancelled. The units
        /// cannot be assigned but can still be reactivated before they are
        /// deleted.
        /// </summary>
        [JsonProperty(PropertyName = "suspended")]
        public int? Suspended { get; set; }

        /// <summary>
        /// Gets or sets the number of units that are in warning status. When
        /// the subscription of the service SKU has expired, the customer has a
        /// grace period to renew their subscription before it is cancelled
        /// (moved to a suspended state).
        /// </summary>
        [JsonProperty(PropertyName = "warning")]
        public int? Warning { get; set; }
    }
}
