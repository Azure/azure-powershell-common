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
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Parameters that define the create packet capture operation.
    /// </summary>
    [Rest.Serialization.JsonTransformation]
    public partial class PacketCapture
    {
        /// <summary>
        /// Initializes a new instance of the PacketCapture class.
        /// </summary>
        public PacketCapture()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the PacketCapture class.
        /// </summary>
        /// <param name="target">The ID of the targeted resource, only VM is
        /// currently supported.</param>
        /// <param name="storageLocation"></param>
        /// <param name="bytesToCapturePerPacket">Number of bytes captured per
        /// packet, the remaining bytes are truncated.</param>
        /// <param name="totalBytesPerSession">Maximum size of the capture
        /// output.</param>
        /// <param name="timeLimitInSeconds">Maximum duration of the capture
        /// session in seconds.</param>
        /// <param name="filters"></param>
        public PacketCapture(string target, PacketCaptureStorageLocation storageLocation, int? bytesToCapturePerPacket = default(int?), int? totalBytesPerSession = default(int?), int? timeLimitInSeconds = default(int?), IList<PacketCaptureFilter> filters = default(IList<PacketCaptureFilter>))
        {
            Target = target;
            BytesToCapturePerPacket = bytesToCapturePerPacket;
            TotalBytesPerSession = totalBytesPerSession;
            TimeLimitInSeconds = timeLimitInSeconds;
            StorageLocation = storageLocation;
            Filters = filters;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the ID of the targeted resource, only VM is currently
        /// supported.
        /// </summary>
        [JsonProperty(PropertyName = "properties.target")]
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets number of bytes captured per packet, the remaining
        /// bytes are truncated.
        /// </summary>
        [JsonProperty(PropertyName = "properties.bytesToCapturePerPacket")]
        public int? BytesToCapturePerPacket { get; set; }

        /// <summary>
        /// Gets or sets maximum size of the capture output.
        /// </summary>
        [JsonProperty(PropertyName = "properties.totalBytesPerSession")]
        public int? TotalBytesPerSession { get; set; }

        /// <summary>
        /// Gets or sets maximum duration of the capture session in seconds.
        /// </summary>
        [JsonProperty(PropertyName = "properties.timeLimitInSeconds")]
        public int? TimeLimitInSeconds { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "properties.storageLocation")]
        public PacketCaptureStorageLocation StorageLocation { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "properties.filters")]
        public IList<PacketCaptureFilter> Filters { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Target == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Target");
            }
            if (StorageLocation == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "StorageLocation");
            }
        }
    }
}
