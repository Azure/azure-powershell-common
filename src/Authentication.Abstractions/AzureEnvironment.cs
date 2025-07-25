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

using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// A record of metadata necessary to manage assets in a specific azure cloud, including necessary endpoints,
    /// location fo service-specific endpoints, and information for bootstrapping authentication
    /// </summary>
    [Serializable]
    public partial class AzureEnvironment : IAzureEnvironment, IEquatable<AzureEnvironment>
    {
        private const string ArmMetadataEnvVariable = "ARM_CLOUD_METADATA_URL";

        private const string DefaultArmMetaDataEndpoint = "https://management.azure.com/metadata/endpoints?api-version=2022-09-01";
        private const string DisableArmMetaDataEndpoint = "DISABLED";

        internal static IDictionary<string, AzureEnvironment> InitializeBuiltInEnvironments(string armMetadataRequestUri, Action<string> debugLogger = null, Action<string> warningLogger = null, IHttpOperations httpOperations = null)
        {
            IDictionary<string, AzureEnvironment> armAzureEnvironments = new Dictionary<string, AzureEnvironment>(StringComparer.InvariantCultureIgnoreCase);
            try
            {
                if (string.IsNullOrEmpty(armMetadataRequestUri))
                {
                    armMetadataRequestUri = Environment.GetEnvironmentVariable(ArmMetadataEnvVariable);
                    if (string.IsNullOrEmpty(armMetadataRequestUri))
                    {
                        armMetadataRequestUri = DefaultArmMetaDataEndpoint;
                    }
                    else
                    {
                        debugLogger?.Invoke($"Get {armMetadataRequestUri} from environment variable {ArmMetadataEnvVariable}");
                    }
                }
                if (DisableArmMetaDataEndpoint.Equals(armMetadataRequestUri?.Trim().ToUpper()))
                {
                    warningLogger?.Invoke($"Discover feature is disabled.");
                    armMetadataRequestUri = null;
                }
                if (!string.IsNullOrEmpty(armMetadataRequestUri))
                {
                    debugLogger?.Invoke($"Discover environments via {armMetadataRequestUri}");
                    try
                    {
                        var list = InitializeEnvironmentsFromArm(armMetadataRequestUri, httpOperations).ConfigureAwait(false).GetAwaiter().GetResult();
                        foreach (var metadata in list)
                        {
                            var env = MapArmToAzureEnvironment(metadata);
                            env.Type = TypeDiscovered;
                            armAzureEnvironments[env.Name] = env;
                            debugLogger?.Invoke($"Added discovered environment {env.Name}");
                        }
                    }
                    catch (Exception e)
                    {
                        warningLogger?.Invoke($"Cannot discover environments from ARM with URI {armMetadataRequestUri}. Falling back to built-in environments.");
                        debugLogger?.Invoke(e.StackTrace);
                    }
                }
            }
            catch (Exception e)
            {
                warningLogger?.Invoke("Cannot discover environments with error");
                warningLogger?.Invoke(e.StackTrace);
            }

            debugLogger?.Invoke($"Load built-in environments");

            if (!armAzureEnvironments.ContainsKey(EnvironmentName.AzureCloud))
            {
                armAzureEnvironments[EnvironmentName.AzureCloud] = GetBuiltInAzureCloud();
            }
            if (!armAzureEnvironments.ContainsKey(EnvironmentName.AzureChinaCloud))
            {
                armAzureEnvironments[EnvironmentName.AzureChinaCloud] = GetBuiltInChinaAzureCloud();
            }
            if (!armAzureEnvironments.ContainsKey(EnvironmentName.AzureUSGovernment))
            {
                armAzureEnvironments[EnvironmentName.AzureUSGovernment] = GetBuiltInUSGovernmentCloud();
            }

            SetExtendedProperties(armAzureEnvironments);

            return armAzureEnvironments;
        }

        /// <summary>
        /// Initializes cloud metadata dynamically from ARM.
        /// </summary>
        private static async Task<List<ArmMetadata>> InitializeEnvironmentsFromArm(string armMetadataRequestUri, IHttpOperations httpOperations = null)
        {
            if (httpOperations == null)
            {
                httpOperations = HttpClientOperationsFactory.Create().GetHttpOperations();
            }
            var armResponseMessage = await httpOperations.GetAsync(armMetadataRequestUri).ConfigureAwait(false);
            if (armResponseMessage?.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Failed to load cloud metadata from the url {armMetadataRequestUri}.");
            }

            string armMetadataContent = null;

            if (armResponseMessage.Content != null)
            {
                armMetadataContent = await armResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            if (string.IsNullOrEmpty(armMetadataContent))
            {
                throw new Exception($"Failed to load cloud metadata from the url {armMetadataRequestUri}.");
            }

            try
            {
                return JsonConvert.DeserializeObject<List<ArmMetadata>>(armMetadataContent);
            }
            catch
            {
                var metadata = JsonConvert.DeserializeObject<ArmMetadata>(armMetadataContent);
                if (metadata!= null)
                {
                    var armMetadataList = new List<ArmMetadata>();
                    armMetadataList.Add(metadata);
                    return armMetadataList;
                }
            }
            return null;
        }

        /// <summary>
        /// Map ARM metadata schema to the AzureEnvironment object.
        /// </summary>
        /// <param name="armMetadata">ARM cloud metadata</param>
        /// <returns></returns>
        private static AzureEnvironment MapArmToAzureEnvironment(ArmMetadata armMetadata)
        {
            var azureEnvironment = new AzureEnvironment
            {
                Name = armMetadata.Name,
                PublishSettingsFileUrl = GetPublishSettingsFileUrl(armMetadata.Name),
                ServiceManagementUrl = armMetadata.Authentication.Audiences[0],
                ResourceManagerUrl = armMetadata.ResourceManager,
                ManagementPortalUrl = armMetadata.Portal,
                ActiveDirectoryAuthority = armMetadata.Authentication.LoginEndpoint,
                ActiveDirectoryServiceEndpointResourceId = armMetadata.Authentication.Audiences[0],
                StorageEndpointSuffix = armMetadata.Suffixes.Storage,
                GalleryUrl = armMetadata.Gallery,
                SqlDatabaseDnsSuffix = armMetadata.Suffixes.SqlServerHostname,
                GraphUrl = armMetadata.Graph,
                //TODO, ARM endpoint doesn't have TrafficManagerDnsSuffix
                TrafficManagerDnsSuffix = GetTrafficManagerDnsSuffix(armMetadata.Name),
                AzureKeyVaultDnsSuffix = armMetadata.Suffixes.KeyVaultDns,
                //Default ARM endpoint doens't provide KeyVault service resource id. Keep it here just in case.
                AzureKeyVaultServiceEndpointResourceId = GetKeyVaultServiceEndpointResourceId(armMetadata.Name),
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = armMetadata.Suffixes.AzureDataLakeAnalyticsCatalogAndJob,
                AzureDataLakeStoreFileSystemEndpointSuffix = armMetadata.Suffixes.AzureDataLakeStoreFileSystem,
                DataLakeEndpointResourceId = armMetadata.ActiveDirectoryDataLake,
                GraphEndpointResourceId = armMetadata.Graph,
                BatchEndpointResourceId = armMetadata.Batch,
                AdTenant = armMetadata.Authentication.Tenant,
                ContainerRegistryEndpointSuffix = armMetadata.Suffixes.AcrLoginServer
            };

            //We reuse the value of KeyVaultDns
            if (string.IsNullOrEmpty(azureEnvironment.AzureKeyVaultServiceEndpointResourceId))
            {
                azureEnvironment.AzureKeyVaultServiceEndpointResourceId = $"https://{azureEnvironment.AzureKeyVaultDnsSuffix}";
            }

            // There are mismatches between metadata built in Azure PowerShell/CLI and from ARM endpoint.
            // Considering compatibility, below hard coded logic accommodates those mismatches
            // SqlDatabaseDnsSuffix requires value leading with period
            // ServiceManagementUrl as audience needs to end with slash
            if (azureEnvironment.SqlDatabaseDnsSuffix != null && !azureEnvironment.SqlDatabaseDnsSuffix.StartsWith("."))
            {
                azureEnvironment.SqlDatabaseDnsSuffix = "." + azureEnvironment.SqlDatabaseDnsSuffix;
            }
            if (azureEnvironment.ServiceManagementUrl != null && !azureEnvironment.ServiceManagementUrl.EndsWith("/"))
            {
                azureEnvironment.ServiceManagementUrl += "/";
            }

            if (!string.IsNullOrEmpty(armMetadata.MicrosoftGraphResourceId))
            {
                azureEnvironment.SetProperty(ExtendedEndpoint.MicrosoftGraphEndpointResourceId, armMetadata.MicrosoftGraphResourceId);
                // ARM endpoint only gives us graph resource ID (with ending slash "/"),
                // we assume the Url (endpoint to where we send requests) equals the resource ID without the slash
                if (armMetadata.MicrosoftGraphResourceId.EndsWith("/"))
                {
                    azureEnvironment.SetProperty(ExtendedEndpoint.MicrosoftGraphUrl,
                        armMetadata.MicrosoftGraphResourceId.TrimEnd('/'));
                }
            }

            if (!string.IsNullOrEmpty(armMetadata.AttestationResourceId))
            {
                azureEnvironment.SetProperty(ExtendedEndpoint.AzureAttestationServiceEndpointResourceId, armMetadata.AttestationResourceId);
                if (!string.IsNullOrEmpty(armMetadata.Suffixes.AttestationEndpoint))
                {
                    azureEnvironment.SetProperty(ExtendedEndpoint.AzureAttestationServiceEndpointSuffix, armMetadata.Suffixes.AttestationEndpoint);
                }
            }

            if (!string.IsNullOrEmpty(armMetadata.SynapseAnalyticsResourceId))
            {
                azureEnvironment.SetProperty(ExtendedEndpoint.AzureSynapseAnalyticsEndpointResourceId, armMetadata.SynapseAnalyticsResourceId);
                if (!string.IsNullOrEmpty(armMetadata.Suffixes.SynapseAnalytics))
                {
                    azureEnvironment.SetProperty(ExtendedEndpoint.AzureSynapseAnalyticsEndpointSuffix, armMetadata.Suffixes.SynapseAnalytics);
                }
            }

            if (!string.IsNullOrEmpty(armMetadata.LogAnalyticsResourceId))
            {
                azureEnvironment.SetProperty(ExtendedEndpoint.OperationalInsightsEndpointResourceId, armMetadata.LogAnalyticsResourceId);
                azureEnvironment.SetProperty(ExtendedEndpoint.OperationalInsightsEndpoint, $"{armMetadata.LogAnalyticsResourceId}/v1");
            }

            //ManagedHsmServiceEndpointSuffix currently uses Built-in endpoint.
            //In new ArmMedata, ManagedHsmServiceEndpointSuffix is provided as so 'MhsmDns'.
            //But it doesn't' make sense to just refresh ManagedHsmServiceEndpointSuffix from ARM without AzureManagedHsmServiceEndpointResourceId.
            //If we want to refresh AzureManagedHsmServiceEndpointResourceId with reference to ManagedHsmServiceEndpointSuffix,
            //we need to check with Arm team and service team. And so we can do this when we receive the request from the service team.
            //ContainerRegistryEndpointSuffix(AcrLoginServer) is the same case.

            return azureEnvironment;
        }

        /// <summary>
        /// Gets publish setting file URL for a cloud.
        /// Note: Included for sake of parity, should remove if defunct.
        /// </summary>
        /// <param name="cloudName">Cloud name</param>
        /// <returns></returns>
        private static string GetPublishSettingsFileUrl(string cloudName)
        {
            switch (cloudName)
            {
                case EnvironmentName.AzureCloud:
                    return AzureEnvironmentConstants.AzurePublishSettingsFileUrl;
                case EnvironmentName.AzureChinaCloud:
                    return AzureEnvironmentConstants.ChinaPublishSettingsFileUrl;
                case EnvironmentName.AzureUSGovernment:
                    return AzureEnvironmentConstants.USGovernmentPublishSettingsFileUrl;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets the key vault service endpoint resource id for a cloud.
        /// Note: Remove once the ARM metadata endpoint exposes KeyVaultServiceEndpointResourceId.
        /// </summary>
        /// <param name="cloudName">Cloud name</param>
        /// <returns></returns>
        private static string GetKeyVaultServiceEndpointResourceId(string cloudName)
        {
            switch (cloudName)
            {
                case EnvironmentName.AzureCloud:
                    return AzureEnvironmentConstants.AzureKeyVaultServiceEndpointResourceId;
                case EnvironmentName.AzureChinaCloud:
                    return AzureEnvironmentConstants.ChinaKeyVaultServiceEndpointResourceId;
                case EnvironmentName.AzureUSGovernment:
                    return AzureEnvironmentConstants.USGovernmentKeyVaultServiceEndpointResourceId;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets the traffic manager DNS suffix for a cloud.
        /// Note: Remove once the ARM metadata endpoint exposes TrafficManagerDnsSuffix.
        /// </summary>
        /// <param name="cloudName">Cloud name</param>
        /// <returns></returns>
        private static string GetTrafficManagerDnsSuffix(string cloudName)
        {
            switch (cloudName)
            {
                case EnvironmentName.AzureCloud:
                    return AzureEnvironmentConstants.AzureTrafficManagerDnsSuffix;
                case EnvironmentName.AzureChinaCloud:
                    return AzureEnvironmentConstants.ChinaTrafficManagerDnsSuffix;
                case EnvironmentName.AzureUSGovernment:
                    return AzureEnvironmentConstants.USGovernmentTrafficManagerDnsSuffix;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Predefined Microsoft Azure environments
        /// </summary>
        public static IDictionary<string, AzureEnvironment> PublicEnvironments { get; } = new ConcurrentDictionary<string, AzureEnvironment>(StringComparer.InvariantCultureIgnoreCase);

        static AzureEnvironment()
        {
            DiscoverEnvironments();
        }

        /// <summary>
        /// Discover environments and load built-in environments
        /// </summary>
        public static void DiscoverEnvironments(string uri = null, Action<string> debugLogger = null, Action<string> warningLogger = null)
        {
            PublicEnvironments.Clear();
            var environments = InitializeBuiltInEnvironments(uri, debugLogger, warningLogger);
            foreach (var env in environments)
            {
                PublicEnvironments.Add(env.Key, env.Value);
            }
        }

        public AzureEnvironment()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        public AzureEnvironment(IAzureEnvironment other)
        {
            this.CopyFrom(other);
        }

        /// <summary>
        /// The name of the environment
        /// </summary>
        public string Name { get; set; }

        public const string TypeUserDefined = "User-defined";
        public const string TypeDiscovered = "Discovered";
        public const string TypeBuiltIn = "Built-in";

        /// <summary>
        /// Type of environment
        /// </summary>
        public string Type { get; set; } = TypeUserDefined;

        /// <summary>
        /// Whether the environment uses AAD (false) or ADFS (true) authentication
        /// </summary>
        public bool OnPremise { get; set; }

        /// <summary>
        /// The RDFE endpoint
        /// </summary>
        public string ServiceManagementUrl { get; set; }

        /// <summary>
        /// The Azure Resource Manager endpoint
        /// </summary>
        public string ResourceManagerUrl { get; set; }

        /// <summary>
        /// The location fo the AUX portal
        /// </summary>
        public string ManagementPortalUrl { get; set; }

        /// <summary>
        /// The location of the publishsettings fiel download web applciation
        /// </summary>
        public string PublishSettingsFileUrl { get; set; }

        /// <summary>
        /// The authentication endpoint
        /// </summary>
        public string ActiveDirectoryAuthority { get; set; }

        /// <summary>
        /// The uri of the template gallery
        /// </summary>
        public string GalleryUrl { get; set; }

        /// <summary>
        /// The URI of the Azure Active Directory Graph endpoint
        /// </summary>
        public string GraphUrl { get; set; }

        /// <summary>
        /// The token audience need for tokens that target RDFE or ARM endpoints
        /// </summary>
        public string ActiveDirectoryServiceEndpointResourceId { get; set; }

        /// <summary>
        /// The domain name suffix for storage services created in this environment
        /// </summary>
        public string StorageEndpointSuffix { get; set; }

        /// <summary>
        /// The domain name suffix for Sql server created in this environment
        /// </summary>
        public string SqlDatabaseDnsSuffix { get; set; }

        /// <summary>
        /// The domain name suffix for traffic manager endpoints created in this ebvironment
        /// </summary>
        public string TrafficManagerDnsSuffix { get; set; }

        /// <summary>
        /// The domain name suffix for Aure KeyVault vaults created in this environment
        /// </summary>
        public string AzureKeyVaultDnsSuffix { get; set; }

        /// <summary>
        /// The token audience required for communicating with the Azure KeyVault service in this environment
        /// </summary>
        public string AzureKeyVaultServiceEndpointResourceId { get; set; }

        /// <summary>
        /// The token audience required for communicating with the Azure Active Directory Graph service in this environment
        /// </summary>
        public string GraphEndpointResourceId { get; set; }

        /// <summary>
        /// The token audience required for communicating with the Azure Active Directory Data Lake service in this environment
        /// </summary>
        public string DataLakeEndpointResourceId { get; set; }

        /// <summary>
        /// The token audience required for communicating with the Batch service in this enviornment
        /// </summary>
        public string BatchEndpointResourceId { get; set; }

        /// <summary>
        /// The domain name suffix for Azure DataLake Catalog and Job services created in this environment
        /// </summary>
        public string AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix { get; set; }

        /// <summary>
        /// The domain name suffix for Azure DataLake file systems created in this environment
        /// </summary>
        public string AzureDataLakeStoreFileSystemEndpointSuffix { get; set; }

        /// <summary>
        /// The name of the default AdTenant in this environment
        /// </summary>
        public string AdTenant { get; set; }

        /// <summary>
        /// The domain name suffix for Azure Container Registry
        /// </summary>
        public string ContainerRegistryEndpointSuffix { get; set; }
        
        /// <summary>
        /// The set of Azure Version Profiles supported in this environment
        /// </summary>
        public IList<string> VersionProfiles { get; } = new List<string>();

        /// <summary>
        /// Additional environment-specific metadata
        /// </summary>
        public IDictionary<string, string> ExtendedProperties { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public bool Equals(AzureEnvironment other)
        {
            if (other == null)
            {
                return false;
            }

            var unequalItemsDict = other.ExtendedProperties.Where(keyValuePair =>
                    !ExtendedProperties[keyValuePair.Key]
                        .Equals(keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(entry => entry.Key, entry => entry.Value);
            if (unequalItemsDict.Any())
            {
                return false;
            }

            var thisNotOther = VersionProfiles.Except(other.VersionProfiles, StringComparer.OrdinalIgnoreCase).ToList();
            var otherNotThis = other.VersionProfiles.Except(VersionProfiles, StringComparer.OrdinalIgnoreCase).ToList();

            if (thisNotOther.Any() || otherNotThis.Any())
            {
                return false;
            }

            return string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase)
                   && this.OnPremise == other.OnPremise
                   && string.Equals(this.ServiceManagementUrl?.TrimEnd('/'), other.ServiceManagementUrl?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.ResourceManagerUrl?.TrimEnd('/'), other.ResourceManagerUrl?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.ManagementPortalUrl?.TrimEnd('/'), other.ManagementPortalUrl?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.PublishSettingsFileUrl?.TrimEnd('/'), other.PublishSettingsFileUrl?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.ActiveDirectoryAuthority?.TrimEnd('/'), other.ActiveDirectoryAuthority?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.GalleryUrl?.TrimEnd('/'), other.GalleryUrl?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.GraphUrl?.TrimEnd('/'), other.GraphUrl?.TrimEnd('/')?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.ActiveDirectoryServiceEndpointResourceId?.TrimEnd('/'), other.ActiveDirectoryServiceEndpointResourceId?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.StorageEndpointSuffix, other.StorageEndpointSuffix, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.SqlDatabaseDnsSuffix, other.SqlDatabaseDnsSuffix, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.TrafficManagerDnsSuffix, other.TrafficManagerDnsSuffix, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.AzureKeyVaultDnsSuffix, other.AzureKeyVaultDnsSuffix, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.AzureKeyVaultServiceEndpointResourceId?.TrimEnd('/'), other.AzureKeyVaultServiceEndpointResourceId?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.GraphEndpointResourceId?.TrimEnd('/'), other.GraphEndpointResourceId?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.DataLakeEndpointResourceId?.TrimEnd('/'), other.DataLakeEndpointResourceId?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.BatchEndpointResourceId?.TrimEnd('/'), other.BatchEndpointResourceId?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix, other.AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.AzureDataLakeStoreFileSystemEndpointSuffix, other.AzureDataLakeStoreFileSystemEndpointSuffix, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.AdTenant?.TrimEnd('/'), other.AdTenant?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(this.ContainerRegistryEndpointSuffix?.TrimEnd('/'), other.ContainerRegistryEndpointSuffix?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// A set of string constants for each of the known environment values - allows users to specify a particular kind of endpoint by name
        /// </summary>
        public static class Endpoint
        {
            public const string AdTenant = "AdTenant",
                ActiveDirectoryServiceEndpointResourceId = "ActiveDirectoryServiceEndpointResourceId",
                GraphEndpointResourceId = "GraphEndpointResourceId",
                AzureKeyVaultServiceEndpointResourceId = "AzureKeyVaultServiceEndpointResourceId",
                AzureKeyVaultDnsSuffix = "AzureKeyVaultDnsSuffix",
                TrafficManagerDnsSuffix = "TrafficManagerDnsSuffix",
                SqlDatabaseDnsSuffix = "SqlDatabaseDnsSuffix",
                StorageEndpointSuffix = "StorageEndpointSuffix",
                Graph = "Graph",
                Gallery = "Gallery",
                ActiveDirectory = "ActiveDirectory",
                ServiceManagement = "ServiceManagement",
                ResourceManager = "ResourceManager",
                PublishSettingsFileUrl = "PublishSettingsFileUrl",
                ManagementPortalUrl = "ManagementPortalUrl",
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = "AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix",
                AzureDataLakeStoreFileSystemEndpointSuffix = "AzureDataLakeStoreFileSystemEndpointSuffix",
                DataLakeEndpointResourceId = "DataLakeEndpointResourceId",
                ContainerRegistryEndpointSuffix = "ContainerRegistryEndpointSuffix",
                BatchEndpointResourceId = "BatchEndpointResourceId";
        }

        public static class ExtendedEndpoint
        {
            public const string OperationalInsightsEndpointResourceId = "OperationalInsightsEndpointResourceId",
                OperationalInsightsEndpoint = "OperationalInsightsEndpoint",
                ManagedHsmServiceEndpointSuffix = "ManagedHsmServiceEndpointSuffix",
                ManagedHsmServiceEndpointResourceId = "ManagedHsmServiceEndpointResourceId",
                AnalysisServicesEndpointSuffix = "AzureAnalysisServicesEndpointSuffix",
                AnalysisServicesEndpointResourceId = "AnalysisServicesEndpointResourceId",
                AzureAttestationServiceEndpointSuffix = "AzureAttestationServiceEndpointSuffix",
                AzureAttestationServiceEndpointResourceId = "AzureAttestationServiceEndpointResourceId",
                AzureSynapseAnalyticsEndpointSuffix = "AzureSynapseAnalyticsEndpointSuffix",
                AzureSynapseAnalyticsEndpointResourceId = "AzureSynapseAnalyticsEndpointResourceId",
                MicrosoftGraphUrl = "MicrosoftGraphUrl",
                MicrosoftGraphEndpointResourceId = "MicrosoftGraphEndpointResourceId",
                AzurePurviewEndpointSuffix = "AzurePurviewEndpointSuffix",
                AzurePurviewEndpointResourceId = "AzurePurviewEndpointResourceId",
                AzureAppConfigurationEndpointSuffix = "AzureAppConfigurationEndpointSuffix",
                AzureAppConfigurationEndpointResourceId = "AzureAppConfigurationEndpointResourceId",
                ContainerRegistryEndpointResourceId = "ContainerRegistryEndpointResourceId",
                AzureCommunicationEmailEndpointSuffix = "AzureCommunicationEmailEndpointSuffix",
                AzureCommunicationEmailEndpointResourceId = "AzureCommunicationEmailEndpointResourceId",
                AzureSshAuthScope = "AzureSshAuthScope";
        }
    }
}
