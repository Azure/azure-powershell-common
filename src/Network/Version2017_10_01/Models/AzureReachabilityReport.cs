// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.Internal.Network.Version2017_10_01.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Azure reachability report details.
    /// </summary>
    public partial class AzureReachabilityReport
    {
        /// <summary>
        /// Initializes a new instance of the AzureReachabilityReport class.
        /// </summary>
        public AzureReachabilityReport()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AzureReachabilityReport class.
        /// </summary>
        /// <param name="aggregationLevel">The aggregation level of Azure
        /// reachability report. Can be Country, State or City.</param>
        /// <param name="providerLocation"></param>
        /// <param name="reachabilityReport">List of Azure reachability report
        /// items.</param>
        public AzureReachabilityReport(string aggregationLevel, AzureReachabilityReportLocation providerLocation, IList<AzureReachabilityReportItem> reachabilityReport)
        {
            AggregationLevel = aggregationLevel;
            ProviderLocation = providerLocation;
            ReachabilityReport = reachabilityReport;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the aggregation level of Azure reachability report.
        /// Can be Country, State or City.
        /// </summary>
        [JsonProperty(PropertyName = "aggregationLevel")]
        public string AggregationLevel { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "providerLocation")]
        public AzureReachabilityReportLocation ProviderLocation { get; set; }

        /// <summary>
        /// Gets or sets list of Azure reachability report items.
        /// </summary>
        [JsonProperty(PropertyName = "reachabilityReport")]
        public IList<AzureReachabilityReportItem> ReachabilityReport { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (AggregationLevel == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "AggregationLevel");
            }
            if (ProviderLocation == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "ProviderLocation");
            }
            if (ReachabilityReport == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "ReachabilityReport");
            }
            if (ProviderLocation != null)
            {
                ProviderLocation.Validate();
            }
        }
    }
}
