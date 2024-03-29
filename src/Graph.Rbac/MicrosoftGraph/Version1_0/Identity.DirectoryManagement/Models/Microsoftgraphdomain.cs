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

    public partial class Microsoftgraphdomain : Microsoftgraphentity
    {
        /// <summary>
        /// Initializes a new instance of the Microsoftgraphdomain class.
        /// </summary>
        public Microsoftgraphdomain()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Microsoftgraphdomain class.
        /// </summary>
        /// <param name="additionalProperties">Unmatched properties from the
        /// message are deserialized this collection</param>
        /// <param name="id">The unique idenfier for an entity.
        /// Read-only.</param>
        /// <param name="authenticationType">Indicates the configured
        /// authentication type for the domain. The value is either Managed or
        /// Federated. Managed indicates a cloud managed domain where Azure AD
        /// performs user authentication. Federated indicates authentication is
        /// federated with an identity provider such as the tenant's
        /// on-premises Active Directory via Active Directory Federation
        /// Services. This property is read-only and is not nullable.</param>
        /// <param name="availabilityStatus">This property is always null
        /// except when the verify action is used. When the verify action is
        /// used, a domain entity is returned in the response. The
        /// availabilityStatus property of the domain entity in the response is
        /// either AvailableImmediately or
        /// EmailVerifiedDomainTakeoverScheduled.</param>
        /// <param name="domainNameReferences">The objects such as users and
        /// groups that reference the domain ID. Read-only, Nullable. Supports
        /// $expand and $filter by the OData type of objects returned. For
        /// example
        /// /domains/{domainId}/domainNameReferences/microsoft.graph.user and
        /// /domains/{domainId}/domainNameReferences/microsoft.graph.group.</param>
        /// <param name="federationConfiguration">Domain settings configured by
        /// a customer when federated with Azure AD. Supports $expand.</param>
        /// <param name="isAdminManaged">The value of the property is false if
        /// the DNS record management of the domain has been delegated to
        /// Microsoft 365. Otherwise, the value is true. Not nullable</param>
        /// <param name="isDefault">true if this is the default domain that is
        /// used for user creation. There is only one default domain per
        /// company. Not nullable</param>
        /// <param name="isInitial">true if this is the initial domain created
        /// by Microsoft Online Services (companyname.onmicrosoft.com). There
        /// is only one initial domain per company. Not nullable</param>
        /// <param name="isRoot">true if the domain is a verified root domain.
        /// Otherwise, false if the domain is a subdomain or unverified. Not
        /// nullable</param>
        /// <param name="isVerified">true if the domain has completed domain
        /// ownership verification. Not nullable</param>
        /// <param name="passwordNotificationWindowInDays">Specifies the number
        /// of days before a user receives notification that their password
        /// will expire. If the property is not set, a default value of 14 days
        /// will be used.</param>
        /// <param name="passwordValidityPeriodInDays">Specifies the length of
        /// time that a password is valid before it must be changed. If the
        /// property is not set, a default value of 90 days will be
        /// used.</param>
        /// <param name="serviceConfigurationRecords">DNS records the customer
        /// adds to the DNS zone file of the domain before the domain can be
        /// used by Microsoft Online services. Read-only, Nullable. Supports
        /// $expand.</param>
        /// <param name="supportedServices">The capabilities assigned to the
        /// domain. Can include 0, 1 or more of following values: Email,
        /// Sharepoint, EmailInternalRelayOnly, OfficeCommunicationsOnline,
        /// SharePointDefaultDomain, FullRedelegation, SharePointPublic,
        /// OrgIdAuthentication, Yammer, Intune. The values which you can
        /// add/remove using Graph API include: Email,
        /// OfficeCommunicationsOnline, Yammer. Not nullable.</param>
        /// <param name="verificationDnsRecords">DNS records that the customer
        /// adds to the DNS zone file of the domain before the customer can
        /// complete domain ownership verification with Azure AD. Read-only,
        /// Nullable. Supports $expand.</param>
        public Microsoftgraphdomain(IDictionary<string, object> additionalProperties = default(IDictionary<string, object>), string id = default(string), string authenticationType = default(string), string availabilityStatus = default(string), IList<MicrosoftgraphdirectoryObject> domainNameReferences = default(IList<MicrosoftgraphdirectoryObject>), IList<MicrosoftgraphinternalDomainFederation> federationConfiguration = default(IList<MicrosoftgraphinternalDomainFederation>), bool? isAdminManaged = default(bool?), bool? isDefault = default(bool?), bool? isInitial = default(bool?), bool? isRoot = default(bool?), bool? isVerified = default(bool?), string manufacturer = default(string), string model = default(string), int? passwordNotificationWindowInDays = default(int?), int? passwordValidityPeriodInDays = default(int?), IList<MicrosoftgraphdomainDnsRecord> serviceConfigurationRecords = default(IList<MicrosoftgraphdomainDnsRecord>), MicrosoftgraphdomainState state = default(MicrosoftgraphdomainState), IList<string> supportedServices = default(IList<string>), IList<MicrosoftgraphdomainDnsRecord> verificationDnsRecords = default(IList<MicrosoftgraphdomainDnsRecord>))
            : base(additionalProperties, id)
        {
            AuthenticationType = authenticationType;
            AvailabilityStatus = availabilityStatus;
            DomainNameReferences = domainNameReferences;
            FederationConfiguration = federationConfiguration;
            IsAdminManaged = isAdminManaged;
            IsDefault = isDefault;
            IsInitial = isInitial;
            IsRoot = isRoot;
            IsVerified = isVerified;
            Manufacturer = manufacturer;
            Model = model;
            PasswordNotificationWindowInDays = passwordNotificationWindowInDays;
            PasswordValidityPeriodInDays = passwordValidityPeriodInDays;
            ServiceConfigurationRecords = serviceConfigurationRecords;
            State = state;
            SupportedServices = supportedServices;
            VerificationDnsRecords = verificationDnsRecords;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets indicates the configured authentication type for the
        /// domain. The value is either Managed or Federated. Managed indicates
        /// a cloud managed domain where Azure AD performs user authentication.
        /// Federated indicates authentication is federated with an identity
        /// provider such as the tenant's on-premises Active Directory via
        /// Active Directory Federation Services. This property is read-only
        /// and is not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "authenticationType")]
        public string AuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets this property is always null except when the verify
        /// action is used. When the verify action is used, a domain entity is
        /// returned in the response. The availabilityStatus property of the
        /// domain entity in the response is either AvailableImmediately or
        /// EmailVerifiedDomainTakeoverScheduled.
        /// </summary>
        [JsonProperty(PropertyName = "availabilityStatus")]
        public string AvailabilityStatus { get; set; }

        /// <summary>
        /// Gets or sets the objects such as users and groups that reference
        /// the domain ID. Read-only, Nullable. Supports $expand and $filter by
        /// the OData type of objects returned. For example
        /// /domains/{domainId}/domainNameReferences/microsoft.graph.user and
        /// /domains/{domainId}/domainNameReferences/microsoft.graph.group.
        /// </summary>
        [JsonProperty(PropertyName = "domainNameReferences")]
        public IList<MicrosoftgraphdirectoryObject> DomainNameReferences { get; set; }

        /// <summary>
        /// Gets or sets domain settings configured by a customer when
        /// federated with Azure AD. Supports $expand.
        /// </summary>
        [JsonProperty(PropertyName = "federationConfiguration")]
        public IList<MicrosoftgraphinternalDomainFederation> FederationConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the value of the property is false if the DNS record
        /// management of the domain has been delegated to Microsoft 365.
        /// Otherwise, the value is true. Not nullable
        /// </summary>
        [JsonProperty(PropertyName = "isAdminManaged")]
        public bool? IsAdminManaged { get; set; }

        /// <summary>
        /// Gets or sets true if this is the default domain that is used for
        /// user creation. There is only one default domain per company. Not
        /// nullable
        /// </summary>
        [JsonProperty(PropertyName = "isDefault")]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Gets or sets true if this is the initial domain created by
        /// Microsoft Online Services (companyname.onmicrosoft.com). There is
        /// only one initial domain per company. Not nullable
        /// </summary>
        [JsonProperty(PropertyName = "isInitial")]
        public bool? IsInitial { get; set; }

        /// <summary>
        /// Gets or sets true if the domain is a verified root domain.
        /// Otherwise, false if the domain is a subdomain or unverified. Not
        /// nullable
        /// </summary>
        [JsonProperty(PropertyName = "isRoot")]
        public bool? IsRoot { get; set; }

        /// <summary>
        /// Gets or sets true if the domain has completed domain ownership
        /// verification. Not nullable
        /// </summary>
        [JsonProperty(PropertyName = "isVerified")]
        public bool? IsVerified { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "manufacturer")]
        public string Manufacturer { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets specifies the number of days before a user receives
        /// notification that their password will expire. If the property is
        /// not set, a default value of 14 days will be used.
        /// </summary>
        [JsonProperty(PropertyName = "passwordNotificationWindowInDays")]
        public int? PasswordNotificationWindowInDays { get; set; }

        /// <summary>
        /// Gets or sets specifies the length of time that a password is valid
        /// before it must be changed. If the property is not set, a default
        /// value of 90 days will be used.
        /// </summary>
        [JsonProperty(PropertyName = "passwordValidityPeriodInDays")]
        public int? PasswordValidityPeriodInDays { get; set; }

        /// <summary>
        /// Gets or sets DNS records the customer adds to the DNS zone file of
        /// the domain before the domain can be used by Microsoft Online
        /// services. Read-only, Nullable. Supports $expand.
        /// </summary>
        [JsonProperty(PropertyName = "serviceConfigurationRecords")]
        public IList<MicrosoftgraphdomainDnsRecord> ServiceConfigurationRecords { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public MicrosoftgraphdomainState State { get; set; }

        /// <summary>
        /// Gets or sets the capabilities assigned to the domain. Can include
        /// 0, 1 or more of following values: Email, Sharepoint,
        /// EmailInternalRelayOnly, OfficeCommunicationsOnline,
        /// SharePointDefaultDomain, FullRedelegation, SharePointPublic,
        /// OrgIdAuthentication, Yammer, Intune. The values which you can
        /// add/remove using Graph API include: Email,
        /// OfficeCommunicationsOnline, Yammer. Not nullable.
        /// </summary>
        [JsonProperty(PropertyName = "supportedServices")]
        public IList<string> SupportedServices { get; set; }

        /// <summary>
        /// Gets or sets DNS records that the customer adds to the DNS zone
        /// file of the domain before the customer can complete domain
        /// ownership verification with Azure AD. Read-only, Nullable. Supports
        /// $expand.
        /// </summary>
        [JsonProperty(PropertyName = "verificationDnsRecords")]
        public IList<MicrosoftgraphdomainDnsRecord> VerificationDnsRecords { get; set; }
    }
}
