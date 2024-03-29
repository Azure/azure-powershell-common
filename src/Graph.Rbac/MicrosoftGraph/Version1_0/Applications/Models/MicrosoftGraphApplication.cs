// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Commands.Common.MSGraph.Version1_0.Applications.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class MicrosoftGraphApplication : MicrosoftGraphDirectoryObject
    {
        /// <summary>
        /// Initializes a new instance of the MicrosoftGraphApplication class.
        /// </summary>
        public MicrosoftGraphApplication()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MicrosoftGraphApplication class.
        /// </summary>
        /// <param name="additionalProperties">Unmatched properties from the
        /// message are deserialized this collection</param>
        /// <param name="id">Read-only.</param>
        /// <param name="deletedDateTime"></param>
        /// <param name="tags">Custom strings that can be used to categorize
        /// and identify the application. Not nullable.Supports $filter (eq,
        /// NOT, ge, le, startsWith).</param>
        /// <param name="appId">The unique identifier for the application that
        /// is assigned by Azure AD. Not nullable. Read-only.</param>
        /// <param name="appRoles">The collection of roles assigned to the
        /// application. With app role assignments, these roles can be assigned
        /// to users, groups, or service principals associated with other
        /// applications. Not nullable.</param>
        /// <param name="applicationTemplateId">Unique identifier of the
        /// applicationTemplate.</param>
        /// <param name="createdDateTime">The date and time the application was
        /// registered. The DateTimeOffset type represents date and time
        /// information using ISO 8601 format and is always in UTC time. For
        /// example, midnight UTC on Jan 1, 2014 is 2014-01-01T00:00:00Z.
        /// Read-only.  Supports $filter (eq, ne, NOT, ge, le, in) and
        /// $orderBy.</param>
        /// <param name="description">An optional description of the
        /// application. Returned by default. Supports $filter (eq, ne, NOT,
        /// ge, le, startsWith) and $search.</param>
        /// <param name="displayName">The display name for the application.
        /// Supports $filter (eq, ne, NOT, ge, le, in, startsWith), $search,
        /// and $orderBy.</param>
        /// <param name="extensionProperties">Read-only. Nullable.</param>
        /// <param name="identifierUris">The URIs that identify the application
        /// within its Azure AD tenant, or within a verified custom domain if
        /// the application is multi-tenant. For more information, see
        /// Application Objects and Service Principal Objects. The any operator
        /// is required for filter expressions on multi-valued properties. Not
        /// nullable. Supports $filter (eq, ne, ge, le, startsWith).</param>
        /// <param name="isDeviceOnlyAuthSupported">Specifies whether this
        /// application supports device authentication without a user. The
        /// default is false.</param>
        /// <param name="keyCredentials">The collection of key credentials
        /// associated with the application. Not nullable. Supports $filter
        /// (eq, NOT, ge, le).</param>
        /// <param name="oauth2RequirePostResponse"></param>
        /// <param name="owners">Directory objects that are owners of the
        /// application. Read-only. Nullable. Supports $expand.</param>
        /// <param name="passwordCredentials">The collection of password
        /// credentials associated with the application. Not nullable.</param>
        public MicrosoftGraphApplication(IDictionary<string, object> additionalProperties = default(IDictionary<string, object>), string id = default(string), System.DateTime? deletedDateTime = default(System.DateTime?), IList<string> tags = default(IList<string>), string appId = default(string), IList<MicrosoftGraphAppRole> appRoles = default(IList<MicrosoftGraphAppRole>), string applicationTemplateId = default(string), System.DateTime? createdDateTime = default(System.DateTime?), string description = default(string), string displayName = default(string), IList<MicrosoftGraphExtensionProperty> extensionProperties = default(IList<MicrosoftGraphExtensionProperty>), IList<string> identifierUris = default(IList<string>), bool? isDeviceOnlyAuthSupported = default(bool?), IList<MicrosoftGraphKeyCredential> keyCredentials = default(IList<MicrosoftGraphKeyCredential>), bool? oauth2RequirePostResponse = default(bool?), IList<MicrosoftGraphDirectoryObject> owners = default(IList<MicrosoftGraphDirectoryObject>), IList<MicrosoftGraphPasswordCredential> passwordCredentials = default(IList<MicrosoftGraphPasswordCredential>))
            : base(additionalProperties, id, deletedDateTime)
        {
            Tags = tags;
            AppId = appId;
            AppRoles = appRoles;
            ApplicationTemplateId = applicationTemplateId;
            CreatedDateTime = createdDateTime;
            Description = description;
            DisplayName = displayName;
            ExtensionProperties = extensionProperties;
            IdentifierUris = identifierUris;
            IsDeviceOnlyAuthSupported = isDeviceOnlyAuthSupported;
            KeyCredentials = keyCredentials;
            Oauth2RequirePostResponse = oauth2RequirePostResponse;
            Owners = owners;
            PasswordCredentials = passwordCredentials;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets custom strings that can be used to categorize and
        /// identify the application. Not nullable.Supports $filter (eq, NOT,
        /// ge, le, startsWith).
        /// </summary>
        [JsonProperty(PropertyName = "tags")]
        public IList<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the application that is
        /// assigned by Azure AD. Not nullable. Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "appId")]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the collection of roles assigned to the application.
        /// With app role assignments, these roles can be assigned to users,
        /// groups, or service principals associated with other applications.
        /// Not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "appRoles")]
        public IList<MicrosoftGraphAppRole> AppRoles { get; set; }

        /// <summary>
        /// Gets or sets unique identifier of the applicationTemplate.
        /// </summary>
        [JsonProperty(PropertyName = "applicationTemplateId")]
        public string ApplicationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the date and time the application was registered. The
        /// DateTimeOffset type represents date and time information using ISO
        /// 8601 format and is always in UTC time. For example, midnight UTC on
        /// Jan 1, 2014 is 2014-01-01T00:00:00Z. Read-only.  Supports $filter
        /// (eq, ne, NOT, ge, le, in) and $orderBy.
        /// </summary>
        [JsonProperty(PropertyName = "createdDateTime")]
        public System.DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets an optional description of the application. Returned
        /// by default. Supports $filter (eq, ne, NOT, ge, le, startsWith) and
        /// $search.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display name for the application. Supports $filter
        /// (eq, ne, NOT, ge, le, in, startsWith), $search, and $orderBy.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets read-only. Nullable.
        /// </summary>
        [JsonProperty(PropertyName = "extensionProperties")]
        public IList<MicrosoftGraphExtensionProperty> ExtensionProperties { get; set; }

        /// <summary>
        /// Gets or sets the URIs that identify the application within its
        /// Azure AD tenant, or within a verified custom domain if the
        /// application is multi-tenant. For more information, see Application
        /// Objects and Service Principal Objects. The any operator is required
        /// for filter expressions on multi-valued properties. Not nullable.
        /// Supports $filter (eq, ne, ge, le, startsWith).
        /// </summary>
        [JsonProperty(PropertyName = "identifierUris")]
        public IList<string> IdentifierUris { get; set; }

        /// <summary>
        /// Gets or sets specifies whether this application supports device
        /// authentication without a user. The default is false.
        /// </summary>
        [JsonProperty(PropertyName = "isDeviceOnlyAuthSupported")]
        public bool? IsDeviceOnlyAuthSupported { get; set; }

        /// <summary>
        /// Gets or sets the collection of key credentials associated with the
        /// application. Not nullable. Supports $filter (eq, NOT, ge, le).
        /// </summary>
        [JsonProperty(PropertyName = "keyCredentials")]
        public IList<MicrosoftGraphKeyCredential> KeyCredentials { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "oauth2RequirePostResponse")]
        public bool? Oauth2RequirePostResponse { get; set; }

        /// <summary>
        /// Gets or sets directory objects that are owners of the application.
        /// Read-only. Nullable. Supports $expand.
        /// </summary>
        [JsonProperty(PropertyName = "owners")]
        public IList<MicrosoftGraphDirectoryObject> Owners { get; set; }

        /// <summary>
        /// Gets or sets the collection of password credentials associated with
        /// the application. Not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "passwordCredentials")]
        public IList<MicrosoftGraphPasswordCredential> PasswordCredentials { get; set; }

    }
}
