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

    /// <summary>
    /// Exception thrown for an invalid response with
    /// MicrosoftgraphODataErrorsODataError information.
    /// </summary>
    public partial class MicrosoftgraphODataErrorsODataErrorException : RestException
    {
        /// <summary>
        /// Gets information about the associated HTTP request.
        /// </summary>
        public HttpRequestMessageWrapper Request { get; set; }

        /// <summary>
        /// Gets information about the associated HTTP response.
        /// </summary>
        public HttpResponseMessageWrapper Response { get; set; }

        /// <summary>
        /// Gets or sets the body object.
        /// </summary>
        public MicrosoftgraphODataErrorsODataError Body { get; set; }

        /// <summary>
        /// Initializes a new instance of the MicrosoftgraphODataErrorsODataErrorException class.
        /// </summary>
        public MicrosoftgraphODataErrorsODataErrorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MicrosoftgraphODataErrorsODataErrorException class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public MicrosoftgraphODataErrorsODataErrorException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MicrosoftgraphODataErrorsODataErrorException class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public MicrosoftgraphODataErrorsODataErrorException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
