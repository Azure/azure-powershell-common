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

    public partial class Microsoftgraphorganization : MicrosoftgraphdirectoryObject
    {
        /// <summary>
        /// Initializes a new instance of the Microsoftgraphorganization class.
        /// </summary>
        public Microsoftgraphorganization()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Microsoftgraphorganization class.
        /// </summary>
        /// <param name="additionalProperties">Unmatched properties from the
        /// message are deserialized this collection</param>
        /// <param name="id">The unique idenfier for an entity.
        /// Read-only.</param>
        /// <param name="deletedDateTime">Date and time when this object was
        /// deleted. Always null when the object hasn't been deleted.</param>
        /// <param name="assignedPlans">The collection of service plans
        /// associated with the tenant. Not nullable.</param>
        /// <param name="businessPhones">Telephone number for the organization.
        /// Although this is a string collection, only one number can be set
        /// for this property.</param>
        /// <param name="certificateBasedAuthConfiguration">Navigation property
        /// to manage certificate-based authentication configuration. Only a
        /// single instance of certificateBasedAuthConfiguration can be created
        /// in the collection.</param>
        /// <param name="city">City name of the address for the
        /// organization.</param>
        /// <param name="country">Country/region name of the address for the
        /// organization.</param>
        /// <param name="countryLetterCode">Country or region abbreviation for
        /// the organization in ISO 3166-2 format.</param>
        /// <param name="createdDateTime">Timestamp of when the organization
        /// was created. The value cannot be modified and is automatically
        /// populated when the organization is created. The Timestamp type
        /// represents date and time information using ISO 8601 format and is
        /// always in UTC time. For example, midnight UTC on Jan 1, 2014 is
        /// 2014-01-01T00:00:00Z. Read-only.</param>
        /// <param name="displayName">The display name for the tenant.</param>
        /// <param name="extensions">The collection of open extensions defined
        /// for the organization. Read-only. Nullable.</param>
        /// <param name="marketingNotificationEmails">Not nullable.</param>
        /// <param name="mobileDeviceManagementAuthority">Possible values
        /// include: 'unknown', 'intune', 'sccm', 'office365'</param>
        /// <param name="onPremisesLastSyncDateTime">The time and date at which
        /// the tenant was last synced with the on-premises directory. The
        /// Timestamp type represents date and time information using ISO 8601
        /// format and is always in UTC time. For example, midnight UTC on Jan
        /// 1, 2014 is 2014-01-01T00:00:00Z. Read-only.</param>
        /// <param name="onPremisesSyncEnabled">true if this object is synced
        /// from an on-premises directory; false if this object was originally
        /// synced from an on-premises directory but is no longer synced.
        /// Nullable. null if this object has never been synced from an
        /// on-premises directory (default).</param>
        /// <param name="postalCode">Postal code of the address for the
        /// organization.</param>
        /// <param name="preferredLanguage">The preferred language for the
        /// organization. Should follow ISO 639-1 Code; for example,
        /// en.</param>
        /// <param name="provisionedPlans">Not nullable.</param>
        /// <param name="state">State name of the address for the
        /// organization.</param>
        /// <param name="street">Street name of the address for
        /// organization.</param>
        /// <param name="technicalNotificationMails">Not nullable.</param>
        /// <param name="verifiedDomains">The collection of domains associated
        /// with this tenant. Not nullable.</param>
        public Microsoftgraphorganization(IDictionary<string, object> additionalProperties = default(IDictionary<string, object>), string id = default(string), System.DateTime? deletedDateTime = default(System.DateTime?), IList<MicrosoftgraphassignedPlan> assignedPlans = default(IList<MicrosoftgraphassignedPlan>), MicrosoftgraphorganizationalBranding branding = default(MicrosoftgraphorganizationalBranding), IList<string> businessPhones = default(IList<string>), IList<MicrosoftgraphcertificateBasedAuthConfiguration> certificateBasedAuthConfiguration = default(IList<MicrosoftgraphcertificateBasedAuthConfiguration>), string city = default(string), string country = default(string), string countryLetterCode = default(string), System.DateTime? createdDateTime = default(System.DateTime?), string displayName = default(string), IList<Microsoftgraphextension> extensions = default(IList<Microsoftgraphextension>), IList<string> marketingNotificationEmails = default(IList<string>), MdmAuthority? mobileDeviceManagementAuthority = default(MdmAuthority?), System.DateTime? onPremisesLastSyncDateTime = default(System.DateTime?), bool? onPremisesSyncEnabled = default(bool?), string postalCode = default(string), string preferredLanguage = default(string), MicrosoftgraphprivacyProfile privacyProfile = default(MicrosoftgraphprivacyProfile), IList<MicrosoftgraphprovisionedPlan> provisionedPlans = default(IList<MicrosoftgraphprovisionedPlan>), IList<string> securityComplianceNotificationMails = default(IList<string>), IList<string> securityComplianceNotificationPhones = default(IList<string>), string state = default(string), string street = default(string), IList<string> technicalNotificationMails = default(IList<string>), string tenantType = default(string), IList<MicrosoftgraphverifiedDomain> verifiedDomains = default(IList<MicrosoftgraphverifiedDomain>))
            : base(additionalProperties, id, deletedDateTime)
        {
            AssignedPlans = assignedPlans;
            Branding = branding;
            BusinessPhones = businessPhones;
            CertificateBasedAuthConfiguration = certificateBasedAuthConfiguration;
            City = city;
            Country = country;
            CountryLetterCode = countryLetterCode;
            CreatedDateTime = createdDateTime;
            DisplayName = displayName;
            Extensions = extensions;
            MarketingNotificationEmails = marketingNotificationEmails;
            MobileDeviceManagementAuthority = mobileDeviceManagementAuthority;
            OnPremisesLastSyncDateTime = onPremisesLastSyncDateTime;
            OnPremisesSyncEnabled = onPremisesSyncEnabled;
            PostalCode = postalCode;
            PreferredLanguage = preferredLanguage;
            PrivacyProfile = privacyProfile;
            ProvisionedPlans = provisionedPlans;
            SecurityComplianceNotificationMails = securityComplianceNotificationMails;
            SecurityComplianceNotificationPhones = securityComplianceNotificationPhones;
            State = state;
            Street = street;
            TechnicalNotificationMails = technicalNotificationMails;
            TenantType = tenantType;
            VerifiedDomains = verifiedDomains;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the collection of service plans associated with the
        /// tenant. Not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "assignedPlans")]
        public IList<MicrosoftgraphassignedPlan> AssignedPlans { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "branding")]
        public MicrosoftgraphorganizationalBranding Branding { get; set; }

        /// <summary>
        /// Gets or sets telephone number for the organization. Although this
        /// is a string collection, only one number can be set for this
        /// property.
        /// </summary>
        [JsonProperty(PropertyName = "businessPhones")]
        public IList<string> BusinessPhones { get; set; }

        /// <summary>
        /// Gets or sets navigation property to manage certificate-based
        /// authentication configuration. Only a single instance of
        /// certificateBasedAuthConfiguration can be created in the collection.
        /// </summary>
        [JsonProperty(PropertyName = "certificateBasedAuthConfiguration")]
        public IList<MicrosoftgraphcertificateBasedAuthConfiguration> CertificateBasedAuthConfiguration { get; set; }

        /// <summary>
        /// Gets or sets city name of the address for the organization.
        /// </summary>
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets country/region name of the address for the
        /// organization.
        /// </summary>
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets country or region abbreviation for the organization in
        /// ISO 3166-2 format.
        /// </summary>
        [JsonProperty(PropertyName = "countryLetterCode")]
        public string CountryLetterCode { get; set; }

        /// <summary>
        /// Gets or sets timestamp of when the organization was created. The
        /// value cannot be modified and is automatically populated when the
        /// organization is created. The Timestamp type represents date and
        /// time information using ISO 8601 format and is always in UTC time.
        /// For example, midnight UTC on Jan 1, 2014 is 2014-01-01T00:00:00Z.
        /// Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "createdDateTime")]
        public System.DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the display name for the tenant.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the collection of open extensions defined for the
        /// organization. Read-only. Nullable.
        /// </summary>
        [JsonProperty(PropertyName = "extensions")]
        public IList<Microsoftgraphextension> Extensions { get; set; }

        /// <summary>
        /// Gets or sets not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "marketingNotificationEmails")]
        public IList<string> MarketingNotificationEmails { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'unknown', 'intune', 'sccm',
        /// 'office365'
        /// </summary>
        [JsonProperty(PropertyName = "mobileDeviceManagementAuthority")]
        public MdmAuthority? MobileDeviceManagementAuthority { get; set; }

        /// <summary>
        /// Gets or sets the time and date at which the tenant was last synced
        /// with the on-premises directory. The Timestamp type represents date
        /// and time information using ISO 8601 format and is always in UTC
        /// time. For example, midnight UTC on Jan 1, 2014 is
        /// 2014-01-01T00:00:00Z. Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "onPremisesLastSyncDateTime")]
        public System.DateTime? OnPremisesLastSyncDateTime { get; set; }

        /// <summary>
        /// Gets or sets true if this object is synced from an on-premises
        /// directory; false if this object was originally synced from an
        /// on-premises directory but is no longer synced. Nullable. null if
        /// this object has never been synced from an on-premises directory
        /// (default).
        /// </summary>
        [JsonProperty(PropertyName = "onPremisesSyncEnabled")]
        public bool? OnPremisesSyncEnabled { get; set; }

        /// <summary>
        /// Gets or sets postal code of the address for the organization.
        /// </summary>
        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the preferred language for the organization. Should
        /// follow ISO 639-1 Code; for example, en.
        /// </summary>
        [JsonProperty(PropertyName = "preferredLanguage")]
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "privacyProfile")]
        public MicrosoftgraphprivacyProfile PrivacyProfile { get; set; }

        /// <summary>
        /// Gets or sets not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "provisionedPlans")]
        public IList<MicrosoftgraphprovisionedPlan> ProvisionedPlans { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "securityComplianceNotificationMails")]
        public IList<string> SecurityComplianceNotificationMails { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "securityComplianceNotificationPhones")]
        public IList<string> SecurityComplianceNotificationPhones { get; set; }

        /// <summary>
        /// Gets or sets state name of the address for the organization.
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets street name of the address for organization.
        /// </summary>
        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "technicalNotificationMails")]
        public IList<string> TechnicalNotificationMails { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "tenantType")]
        public string TenantType { get; set; }

        /// <summary>
        /// Gets or sets the collection of domains associated with this tenant.
        /// Not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "verifiedDomains")]
        public IList<MicrosoftgraphverifiedDomain> VerifiedDomains { get; set; }
    }
}
