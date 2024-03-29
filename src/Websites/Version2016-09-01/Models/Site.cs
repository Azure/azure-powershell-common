// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.WebSites.Version2016_09_01.Models
{
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A web app, a mobile app backend, or an API app.
    /// </summary>
    [Rest.Serialization.JsonTransformation]
    public partial class Site : Resource
    {
        /// <summary>
        /// Initializes a new instance of the Site class.
        /// </summary>
        public Site()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Site class.
        /// </summary>
        /// <param name="location">Resource Location.</param>
        /// <param name="id">Resource Id.</param>
        /// <param name="name">Resource Name.</param>
        /// <param name="kind">Kind of resource.</param>
        /// <param name="type">Resource type.</param>
        /// <param name="tags">Resource tags.</param>
        /// <param name="state">Current state of the app.</param>
        /// <param name="hostNames">Hostnames associated with the app.</param>
        /// <param name="repositorySiteName">Name of the repository
        /// site.</param>
        /// <param name="usageState">State indicating whether the app has
        /// exceeded its quota usage. Read-only. Possible values include:
        /// 'Normal', 'Exceeded'</param>
        /// <param name="enabled">&lt;code&gt;true&lt;/code&gt; if the app is
        /// enabled; otherwise, &lt;code&gt;false&lt;/code&gt;. Setting this
        /// value to false disables the app (takes the app offline).</param>
        /// <param name="enabledHostNames">Enabled hostnames for the
        /// app.Hostnames need to be assigned (see HostNames) AND enabled.
        /// Otherwise,
        /// the app is not served on those hostnames.</param>
        /// <param name="availabilityState">Management information availability
        /// state for the app. Possible values include: 'Normal', 'Limited',
        /// 'DisasterRecoveryMode'</param>
        /// <param name="hostNameSslStates">Hostname SSL states are used to
        /// manage the SSL bindings for app's hostnames.</param>
        /// <param name="serverFarmId">Resource ID of the associated App
        /// Service plan, formatted as:
        /// "/subscriptions/{subscriptionID}/resourceGroups/{groupName}/providers/Microsoft.Web/serverfarms/{appServicePlanName}".</param>
        /// <param name="reserved">&lt;code&gt;true&lt;/code&gt; if reserved;
        /// otherwise, &lt;code&gt;false&lt;/code&gt;.</param>
        /// <param name="lastModifiedTimeUtc">Last time the app was modified,
        /// in UTC. Read-only.</param>
        /// <param name="siteConfig">Configuration of the app.</param>
        /// <param name="trafficManagerHostNames">Azure Traffic Manager
        /// hostnames associated with the app. Read-only.</param>
        /// <param name="scmSiteAlsoStopped">&lt;code&gt;true&lt;/code&gt; to
        /// stop SCM (KUDU) site when the app is stopped; otherwise,
        /// &lt;code&gt;false&lt;/code&gt;. The default is
        /// &lt;code&gt;false&lt;/code&gt;.</param>
        /// <param name="targetSwapSlot">Specifies which deployment slot this
        /// app will swap into. Read-only.</param>
        /// <param name="hostingEnvironmentProfile">App Service Environment to
        /// use for the app.</param>
        /// <param name="clientAffinityEnabled">&lt;code&gt;true&lt;/code&gt;
        /// to enable client affinity; &lt;code&gt;false&lt;/code&gt; to stop
        /// sending session affinity cookies, which route client requests in
        /// the same session to the same instance. Default is
        /// &lt;code&gt;true&lt;/code&gt;.</param>
        /// <param name="clientCertEnabled">&lt;code&gt;true&lt;/code&gt; to
        /// enable client certificate authentication (TLS mutual
        /// authentication); otherwise, &lt;code&gt;false&lt;/code&gt;. Default
        /// is &lt;code&gt;false&lt;/code&gt;.</param>
        /// <param name="hostNamesDisabled">&lt;code&gt;true&lt;/code&gt; to
        /// disable the public hostnames of the app; otherwise,
        /// &lt;code&gt;false&lt;/code&gt;.
        /// If &lt;code&gt;true&lt;/code&gt;, the app is only accessible via
        /// API management process.</param>
        /// <param name="outboundIpAddresses">List of IP addresses that the app
        /// uses for outbound connections (e.g. database access). Includes VIPs
        /// from tenants that site can be hosted with current settings.
        /// Read-only.</param>
        /// <param name="possibleOutboundIpAddresses">List of IP addresses that
        /// the app uses for outbound connections (e.g. database access).
        /// Includes VIPs from all tenants. Read-only.</param>
        /// <param name="containerSize">Size of the function container.</param>
        /// <param name="dailyMemoryTimeQuota">Maximum allowed daily
        /// memory-time quota (applicable on dynamic apps only).</param>
        /// <param name="suspendedTill">App suspended till in case memory-time
        /// quota is exceeded.</param>
        /// <param name="maxNumberOfWorkers">Maximum number of workers.
        /// This only applies to Functions container.</param>
        /// <param name="cloningInfo">If specified during app creation, the app
        /// is cloned from a source app.</param>
        /// <param name="snapshotInfo">If specified during app creation, the
        /// app is created from a previous snapshot.</param>
        /// <param name="resourceGroup">Name of the resource group the app
        /// belongs to. Read-only.</param>
        /// <param name="isDefaultContainer">&lt;code&gt;true&lt;/code&gt; if
        /// the app is a default container; otherwise,
        /// &lt;code&gt;false&lt;/code&gt;.</param>
        /// <param name="defaultHostName">Default hostname of the app.
        /// Read-only.</param>
        /// <param name="slotSwapStatus">Status of the last deployment slot
        /// swap operation.</param>
        /// <param name="httpsOnly">HttpsOnly: configures a web site to accept
        /// only https requests. Issues redirect for
        /// http requests</param>
        /// <param name="identity"></param>
        public Site(string location, string id = default(string), string name = default(string), string kind = default(string), string type = default(string), IDictionary<string, string> tags = default(IDictionary<string, string>), string state = default(string), IList<string> hostNames = default(IList<string>), string repositorySiteName = default(string), UsageState? usageState = default(UsageState?), bool? enabled = default(bool?), IList<string> enabledHostNames = default(IList<string>), SiteAvailabilityState? availabilityState = default(SiteAvailabilityState?), IList<HostNameSslState> hostNameSslStates = default(IList<HostNameSslState>), string serverFarmId = default(string), bool? reserved = default(bool?), System.DateTime? lastModifiedTimeUtc = default(System.DateTime?), SiteConfig siteConfig = default(SiteConfig), IList<string> trafficManagerHostNames = default(IList<string>), bool? scmSiteAlsoStopped = default(bool?), string targetSwapSlot = default(string), HostingEnvironmentProfile hostingEnvironmentProfile = default(HostingEnvironmentProfile), bool? clientAffinityEnabled = default(bool?), bool? clientCertEnabled = default(bool?), bool? hostNamesDisabled = default(bool?), string outboundIpAddresses = default(string), string possibleOutboundIpAddresses = default(string), int? containerSize = default(int?), int? dailyMemoryTimeQuota = default(int?), System.DateTime? suspendedTill = default(System.DateTime?), int? maxNumberOfWorkers = default(int?), CloningInfo cloningInfo = default(CloningInfo), SnapshotRecoveryRequest snapshotInfo = default(SnapshotRecoveryRequest), string resourceGroup = default(string), bool? isDefaultContainer = default(bool?), string defaultHostName = default(string), SlotSwapStatus slotSwapStatus = default(SlotSwapStatus), bool? httpsOnly = default(bool?), ManagedServiceIdentity identity = default(ManagedServiceIdentity))
            : base(location, id, name, kind, type, tags)
        {
            State = state;
            HostNames = hostNames;
            RepositorySiteName = repositorySiteName;
            UsageState = usageState;
            Enabled = enabled;
            EnabledHostNames = enabledHostNames;
            AvailabilityState = availabilityState;
            HostNameSslStates = hostNameSslStates;
            ServerFarmId = serverFarmId;
            Reserved = reserved;
            LastModifiedTimeUtc = lastModifiedTimeUtc;
            SiteConfig = siteConfig;
            TrafficManagerHostNames = trafficManagerHostNames;
            ScmSiteAlsoStopped = scmSiteAlsoStopped;
            TargetSwapSlot = targetSwapSlot;
            HostingEnvironmentProfile = hostingEnvironmentProfile;
            ClientAffinityEnabled = clientAffinityEnabled;
            ClientCertEnabled = clientCertEnabled;
            HostNamesDisabled = hostNamesDisabled;
            OutboundIpAddresses = outboundIpAddresses;
            PossibleOutboundIpAddresses = possibleOutboundIpAddresses;
            ContainerSize = containerSize;
            DailyMemoryTimeQuota = dailyMemoryTimeQuota;
            SuspendedTill = suspendedTill;
            MaxNumberOfWorkers = maxNumberOfWorkers;
            CloningInfo = cloningInfo;
            SnapshotInfo = snapshotInfo;
            ResourceGroup = resourceGroup;
            IsDefaultContainer = isDefaultContainer;
            DefaultHostName = defaultHostName;
            SlotSwapStatus = slotSwapStatus;
            HttpsOnly = httpsOnly;
            Identity = identity;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets current state of the app.
        /// </summary>
        [JsonProperty(PropertyName = "properties.state")]
        public string State { get; private set; }

        /// <summary>
        /// Gets hostnames associated with the app.
        /// </summary>
        [JsonProperty(PropertyName = "properties.hostNames")]
        public IList<string> HostNames { get; private set; }

        /// <summary>
        /// Gets name of the repository site.
        /// </summary>
        [JsonProperty(PropertyName = "properties.repositorySiteName")]
        public string RepositorySiteName { get; private set; }

        /// <summary>
        /// Gets state indicating whether the app has exceeded its quota usage.
        /// Read-only. Possible values include: 'Normal', 'Exceeded'
        /// </summary>
        [JsonProperty(PropertyName = "properties.usageState")]
        public UsageState? UsageState { get; private set; }

        /// <summary>
        /// Gets or sets &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt; if the
        /// app is enabled; otherwise,
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt;. Setting this value
        /// to false disables the app (takes the app offline).
        /// </summary>
        [JsonProperty(PropertyName = "properties.enabled")]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Gets enabled hostnames for the app.Hostnames need to be assigned
        /// (see HostNames) AND enabled. Otherwise,
        /// the app is not served on those hostnames.
        /// </summary>
        [JsonProperty(PropertyName = "properties.enabledHostNames")]
        public IList<string> EnabledHostNames { get; private set; }

        /// <summary>
        /// Gets management information availability state for the app.
        /// Possible values include: 'Normal', 'Limited',
        /// 'DisasterRecoveryMode'
        /// </summary>
        [JsonProperty(PropertyName = "properties.availabilityState")]
        public SiteAvailabilityState? AvailabilityState { get; private set; }

        /// <summary>
        /// Gets or sets hostname SSL states are used to manage the SSL
        /// bindings for app's hostnames.
        /// </summary>
        [JsonProperty(PropertyName = "properties.hostNameSslStates")]
        public IList<HostNameSslState> HostNameSslStates { get; set; }

        /// <summary>
        /// Gets or sets resource ID of the associated App Service plan,
        /// formatted as:
        /// "/subscriptions/{subscriptionID}/resourceGroups/{groupName}/providers/Microsoft.Web/serverfarms/{appServicePlanName}".
        /// </summary>
        [JsonProperty(PropertyName = "properties.serverFarmId")]
        public string ServerFarmId { get; set; }

        /// <summary>
        /// Gets or sets &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt; if
        /// reserved; otherwise,
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt;.
        /// </summary>
        [JsonProperty(PropertyName = "properties.reserved")]
        public bool? Reserved { get; set; }

        /// <summary>
        /// Gets last time the app was modified, in UTC. Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "properties.lastModifiedTimeUtc")]
        public System.DateTime? LastModifiedTimeUtc { get; private set; }

        /// <summary>
        /// Gets or sets configuration of the app.
        /// </summary>
        [JsonProperty(PropertyName = "properties.siteConfig")]
        public SiteConfig SiteConfig { get; set; }

        /// <summary>
        /// Gets azure Traffic Manager hostnames associated with the app.
        /// Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "properties.trafficManagerHostNames")]
        public IList<string> TrafficManagerHostNames { get; private set; }

        /// <summary>
        /// Gets or sets &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt; to stop
        /// SCM (KUDU) site when the app is stopped; otherwise,
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt;. The default is
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt;.
        /// </summary>
        [JsonProperty(PropertyName = "properties.scmSiteAlsoStopped")]
        public bool? ScmSiteAlsoStopped { get; set; }

        /// <summary>
        /// Gets specifies which deployment slot this app will swap into.
        /// Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "properties.targetSwapSlot")]
        public string TargetSwapSlot { get; private set; }

        /// <summary>
        /// Gets or sets app Service Environment to use for the app.
        /// </summary>
        [JsonProperty(PropertyName = "properties.hostingEnvironmentProfile")]
        public HostingEnvironmentProfile HostingEnvironmentProfile { get; set; }

        /// <summary>
        /// Gets or sets &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt; to
        /// enable client affinity;
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt; to stop sending
        /// session affinity cookies, which route client requests in the same
        /// session to the same instance. Default is
        /// &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt;.
        /// </summary>
        [JsonProperty(PropertyName = "properties.clientAffinityEnabled")]
        public bool? ClientAffinityEnabled { get; set; }

        /// <summary>
        /// Gets or sets &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt; to
        /// enable client certificate authentication (TLS mutual
        /// authentication); otherwise,
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt;. Default is
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt;.
        /// </summary>
        [JsonProperty(PropertyName = "properties.clientCertEnabled")]
        public bool? ClientCertEnabled { get; set; }

        /// <summary>
        /// Gets or sets &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt; to
        /// disable the public hostnames of the app; otherwise,
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt;.
        /// If &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt;, the app is only
        /// accessible via API management process.
        /// </summary>
        [JsonProperty(PropertyName = "properties.hostNamesDisabled")]
        public bool? HostNamesDisabled { get; set; }

        /// <summary>
        /// Gets list of IP addresses that the app uses for outbound
        /// connections (e.g. database access). Includes VIPs from tenants that
        /// site can be hosted with current settings. Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "properties.outboundIpAddresses")]
        public string OutboundIpAddresses { get; private set; }

        /// <summary>
        /// Gets list of IP addresses that the app uses for outbound
        /// connections (e.g. database access). Includes VIPs from all tenants.
        /// Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "properties.possibleOutboundIpAddresses")]
        public string PossibleOutboundIpAddresses { get; private set; }

        /// <summary>
        /// Gets or sets size of the function container.
        /// </summary>
        [JsonProperty(PropertyName = "properties.containerSize")]
        public int? ContainerSize { get; set; }

        /// <summary>
        /// Gets or sets maximum allowed daily memory-time quota (applicable on
        /// dynamic apps only).
        /// </summary>
        [JsonProperty(PropertyName = "properties.dailyMemoryTimeQuota")]
        public int? DailyMemoryTimeQuota { get; set; }

        /// <summary>
        /// Gets app suspended till in case memory-time quota is exceeded.
        /// </summary>
        [JsonProperty(PropertyName = "properties.suspendedTill")]
        public System.DateTime? SuspendedTill { get; private set; }

        /// <summary>
        /// Gets maximum number of workers.
        /// This only applies to Functions container.
        /// </summary>
        [JsonProperty(PropertyName = "properties.maxNumberOfWorkers")]
        public int? MaxNumberOfWorkers { get; private set; }

        /// <summary>
        /// Gets or sets if specified during app creation, the app is cloned
        /// from a source app.
        /// </summary>
        [JsonProperty(PropertyName = "properties.cloningInfo")]
        public CloningInfo CloningInfo { get; set; }

        /// <summary>
        /// Gets or sets if specified during app creation, the app is created
        /// from a previous snapshot.
        /// </summary>
        [JsonProperty(PropertyName = "properties.snapshotInfo")]
        public SnapshotRecoveryRequest SnapshotInfo { get; set; }

        /// <summary>
        /// Gets name of the resource group the app belongs to. Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "properties.resourceGroup")]
        public string ResourceGroup { get; private set; }

        /// <summary>
        /// Gets &amp;lt;code&amp;gt;true&amp;lt;/code&amp;gt; if the app is a
        /// default container; otherwise,
        /// &amp;lt;code&amp;gt;false&amp;lt;/code&amp;gt;.
        /// </summary>
        [JsonProperty(PropertyName = "properties.isDefaultContainer")]
        public bool? IsDefaultContainer { get; private set; }

        /// <summary>
        /// Gets default hostname of the app. Read-only.
        /// </summary>
        [JsonProperty(PropertyName = "properties.defaultHostName")]
        public string DefaultHostName { get; private set; }

        /// <summary>
        /// Gets status of the last deployment slot swap operation.
        /// </summary>
        [JsonProperty(PropertyName = "properties.slotSwapStatus")]
        public SlotSwapStatus SlotSwapStatus { get; private set; }

        /// <summary>
        /// Gets or sets httpsOnly: configures a web site to accept only https
        /// requests. Issues redirect for
        /// http requests
        /// </summary>
        [JsonProperty(PropertyName = "properties.httpsOnly")]
        public bool? HttpsOnly { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "identity")]
        public ManagedServiceIdentity Identity { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public override void Validate()
        {
            base.Validate();
            if (SiteConfig != null)
            {
                SiteConfig.Validate();
            }
            if (CloningInfo != null)
            {
                CloningInfo.Validate();
            }
            if (SnapshotInfo != null)
            {
                SnapshotInfo.Validate();
            }
        }
    }
}
