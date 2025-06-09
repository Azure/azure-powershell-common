// ----------------------------------------------------------------------------------
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

using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// The build-in endpoints settings of AzureEnvironment.
    /// </summary>
    public partial class AzureEnvironment
    {

        /// <summary>
        /// Get built-in endpoints of AzureCloud.
        /// </summary>
        private static AzureEnvironment GetBuiltInAzureCloud()
        {
            return new AzureEnvironment
            {
                Name = EnvironmentName.AzureCloud,
                Type = TypeBuiltIn,
                PublishSettingsFileUrl = AzureEnvironmentConstants.AzurePublishSettingsFileUrl,
                ServiceManagementUrl = AzureEnvironmentConstants.AzureServiceEndpoint,
                ResourceManagerUrl = AzureEnvironmentConstants.AzureResourceManagerEndpoint,
                ManagementPortalUrl = AzureEnvironmentConstants.AzureManagementPortalUrl,
                ActiveDirectoryAuthority = AzureEnvironmentConstants.AzureActiveDirectoryEndpoint,
                ActiveDirectoryServiceEndpointResourceId = AzureEnvironmentConstants.AzureServiceEndpoint,
                StorageEndpointSuffix = AzureEnvironmentConstants.AzureStorageEndpointSuffix,
                SqlDatabaseDnsSuffix = AzureEnvironmentConstants.AzureSqlDatabaseDnsSuffix,
                GraphUrl = AzureEnvironmentConstants.AzureGraphEndpoint,
                TrafficManagerDnsSuffix = AzureEnvironmentConstants.AzureTrafficManagerDnsSuffix,
                AzureKeyVaultDnsSuffix = AzureEnvironmentConstants.AzureKeyVaultDnsSuffix,
                AzureKeyVaultServiceEndpointResourceId = AzureEnvironmentConstants.AzureKeyVaultServiceEndpointResourceId,
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = AzureEnvironmentConstants.AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix,
                AzureDataLakeStoreFileSystemEndpointSuffix = AzureEnvironmentConstants.AzureDataLakeStoreFileSystemEndpointSuffix,
                GraphEndpointResourceId = AzureEnvironmentConstants.AzureGraphEndpoint,
                DataLakeEndpointResourceId = AzureEnvironmentConstants.AzureDataLakeServiceEndpointResourceId,
                BatchEndpointResourceId = AzureEnvironmentConstants.BatchEndpointResourceId,
                ContainerRegistryEndpointSuffix = AzureEnvironmentConstants.AzureContainerRegistryEndpointSuffix,
                AdTenant = "Common",

                ExtendedProperties = 
                {
                    { ExtendedEndpoint.AzureAttestationServiceEndpointSuffix, AzureEnvironmentConstants.AzureAttestationServiceEndpointSuffix },
                    { ExtendedEndpoint.AzureAttestationServiceEndpointResourceId, AzureEnvironmentConstants.AzureAttestationServiceEndpointResourceId },
                    { ExtendedEndpoint.MicrosoftGraphEndpointResourceId, AzureEnvironmentConstants.AzureMicrosoftGraphEndpointResourceId },
                    { ExtendedEndpoint.MicrosoftGraphUrl, AzureEnvironmentConstants.AzureMicrosoftGraphUrl },
                    { ExtendedEndpoint.AzureSynapseAnalyticsEndpointSuffix, AzureEnvironmentConstants.AzureSynapseAnalyticsEndpointSuffix },
                    { ExtendedEndpoint.AzureSynapseAnalyticsEndpointResourceId, AzureEnvironmentConstants.AzureSynapseAnalyticsEndpointResourceId }
                }
            };
        }

        /// <summary>
        /// Get built-in endpoints of AzureChinaCloud.
        /// </summary>
        private static AzureEnvironment GetBuiltInChinaAzureCloud()
        {
            return new AzureEnvironment
            {
                Name = EnvironmentName.AzureChinaCloud,
                Type = TypeBuiltIn,
                PublishSettingsFileUrl = AzureEnvironmentConstants.ChinaPublishSettingsFileUrl,
                ServiceManagementUrl = AzureEnvironmentConstants.ChinaServiceEndpoint,
                ResourceManagerUrl = AzureEnvironmentConstants.ChinaResourceManagerEndpoint,
                ManagementPortalUrl = AzureEnvironmentConstants.ChinaManagementPortalUrl,
                ActiveDirectoryAuthority = AzureEnvironmentConstants.ChinaActiveDirectoryEndpoint,
                ActiveDirectoryServiceEndpointResourceId = AzureEnvironmentConstants.ChinaServiceEndpoint,
                StorageEndpointSuffix = AzureEnvironmentConstants.ChinaStorageEndpointSuffix,
                SqlDatabaseDnsSuffix = AzureEnvironmentConstants.ChinaSqlDatabaseDnsSuffix,
                GraphUrl = AzureEnvironmentConstants.ChinaGraphEndpoint,
                TrafficManagerDnsSuffix = AzureEnvironmentConstants.ChinaTrafficManagerDnsSuffix,
                AzureKeyVaultDnsSuffix = AzureEnvironmentConstants.ChinaKeyVaultDnsSuffix,
                AzureKeyVaultServiceEndpointResourceId = AzureEnvironmentConstants.ChinaKeyVaultServiceEndpointResourceId,
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = null,
                AzureDataLakeStoreFileSystemEndpointSuffix = null,
                DataLakeEndpointResourceId = null,
                GraphEndpointResourceId = AzureEnvironmentConstants.ChinaGraphEndpoint,
                BatchEndpointResourceId = AzureEnvironmentConstants.ChinaBatchEndpointResourceId,
                ContainerRegistryEndpointSuffix = AzureEnvironmentConstants.ChinaContainerRegistryEndpointSuffix,
                AdTenant = "Common",

                ExtendedProperties =
                {
                    { ExtendedEndpoint.MicrosoftGraphEndpointResourceId, AzureEnvironmentConstants.ChinaMicrosoftGraphEndpointResourceId },
                    { ExtendedEndpoint.MicrosoftGraphUrl, AzureEnvironmentConstants.ChinaMicrosoftGraphUrl },
                    { ExtendedEndpoint.AzureSynapseAnalyticsEndpointSuffix, AzureEnvironmentConstants.ChinaSynapseAnalyticsEndpointSuffix },
                    { ExtendedEndpoint.AzureSynapseAnalyticsEndpointResourceId, AzureEnvironmentConstants.ChinaSynapseAnalyticsEndpointResourceId }
                }
            };
        }

        /// <summary>
        /// Get built-in endpoints of AzureUSGovernment.
        /// </summary>
        private static AzureEnvironment GetBuiltInUSGovernmentCloud()
        {
            return new AzureEnvironment
            {
                Name = EnvironmentName.AzureUSGovernment,
                Type = TypeBuiltIn,
                PublishSettingsFileUrl = AzureEnvironmentConstants.USGovernmentPublishSettingsFileUrl,
                ServiceManagementUrl = AzureEnvironmentConstants.USGovernmentServiceEndpoint,
                ResourceManagerUrl = AzureEnvironmentConstants.USGovernmentResourceManagerEndpoint,
                ManagementPortalUrl = AzureEnvironmentConstants.USGovernmentManagementPortalUrl,
                ActiveDirectoryAuthority = AzureEnvironmentConstants.USGovernmentActiveDirectoryEndpoint,
                ActiveDirectoryServiceEndpointResourceId = AzureEnvironmentConstants.USGovernmentServiceEndpoint,
                StorageEndpointSuffix = AzureEnvironmentConstants.USGovernmentStorageEndpointSuffix,
                SqlDatabaseDnsSuffix = AzureEnvironmentConstants.USGovernmentSqlDatabaseDnsSuffix,
                GraphUrl = AzureEnvironmentConstants.USGovernmentGraphEndpoint,
                TrafficManagerDnsSuffix = AzureEnvironmentConstants.USGovernmentTrafficManagerDnsSuffix,
                AzureKeyVaultDnsSuffix = AzureEnvironmentConstants.USGovernmentKeyVaultDnsSuffix,
                AzureKeyVaultServiceEndpointResourceId = AzureEnvironmentConstants.USGovernmentKeyVaultServiceEndpointResourceId,
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = null,
                AzureDataLakeStoreFileSystemEndpointSuffix = null,
                DataLakeEndpointResourceId = null,
                GraphEndpointResourceId = AzureEnvironmentConstants.USGovernmentGraphEndpoint,
                BatchEndpointResourceId = AzureEnvironmentConstants.USGovernmentBatchEndpointResourceId,
                ContainerRegistryEndpointSuffix = AzureEnvironmentConstants.USGovernmentContainerRegistryEndpointSuffix,
                AdTenant = "Common",

                ExtendedProperties =
                {
                    { ExtendedEndpoint.MicrosoftGraphEndpointResourceId, AzureEnvironmentConstants.USGovernmentMicrosoftGraphEndpointResourceId },
                    { ExtendedEndpoint.MicrosoftGraphUrl, AzureEnvironmentConstants.USGovernmentMicrosoftGraphUrl },
                    { ExtendedEndpoint.AzureSynapseAnalyticsEndpointSuffix, AzureEnvironmentConstants.USGovernmentSynapseAnalyticsEndpointSuffix },
                    { ExtendedEndpoint.AzureSynapseAnalyticsEndpointResourceId, AzureEnvironmentConstants.USGovernmentSynapseAnalyticsEndpointResourceId }
                }
            };
        }

        /// <summary>
        /// Set Extended properties (to maintain parity).
        /// </summary>
        /// <param name="azureEnvironments">Collection of AzureEnvironments</param>
        private static void SetExtendedProperties(IDictionary<string, AzureEnvironment> azureEnvironments)
        {
            if (azureEnvironments.ContainsKey(EnvironmentName.AzureCloud))
            {
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.OperationalInsightsEndpoint, AzureEnvironmentConstants.AzureOperationalInsightsEndpoint);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.OperationalInsightsEndpointResourceId, AzureEnvironmentConstants.AzureOperationalInsightsEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AnalysisServicesEndpointSuffix, AzureEnvironmentConstants.AzureAnalysisServicesEndpointSuffix);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AnalysisServicesEndpointResourceId, AzureEnvironmentConstants.AzureAnalysisServicesEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.ManagedHsmServiceEndpointResourceId, AzureEnvironmentConstants.AzureManagedHsmServiceEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.ManagedHsmServiceEndpointSuffix, AzureEnvironmentConstants.AzureManagedHsmDnsSuffix);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AzurePurviewEndpointSuffix, AzureEnvironmentConstants.AzurePurviewEndpointSuffix);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AzurePurviewEndpointResourceId, AzureEnvironmentConstants.AzurePurviewEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AzureAppConfigurationEndpointSuffix, AzureEnvironmentConstants.AzureAppConfigurationEndpointSuffix);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AzureAppConfigurationEndpointResourceId, AzureEnvironmentConstants.AzureAppConfigurationEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.ContainerRegistryEndpointResourceId, AzureEnvironmentConstants.AzureContainerRegistryEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AzureCommunicationEmailEndpointSuffix, AzureEnvironmentConstants.AzureCommunicationEmailEndpointSuffix);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AzureCommunicationEmailEndpointResourceId, AzureEnvironmentConstants.AzureCommunicationEmailEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureCloud].SetProperty(ExtendedEndpoint.AzureSshAuthScope, AzureEnvironmentConstants.AzureSshAuthScope);
            }

            if (azureEnvironments.ContainsKey(EnvironmentName.AzureChinaCloud))
            {
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.OperationalInsightsEndpoint, AzureEnvironmentConstants.ChinaOperationalInsightsEndpoint);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.OperationalInsightsEndpointResourceId, AzureEnvironmentConstants.ChinaOperationalInsightsEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.AnalysisServicesEndpointSuffix, AzureEnvironmentConstants.ChinaAnalysisServicesEndpointSuffix);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.AnalysisServicesEndpointResourceId, AzureEnvironmentConstants.ChinaAnalysisServicesEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.ManagedHsmServiceEndpointResourceId, AzureEnvironmentConstants.ChinaManagedHsmServiceEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.ManagedHsmServiceEndpointSuffix, AzureEnvironmentConstants.ChinaManagedHsmDnsSuffix);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.AzureAppConfigurationEndpointResourceId, AzureEnvironmentConstants.ChinaAppConfigurationEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.AzureAppConfigurationEndpointSuffix, AzureEnvironmentConstants.ChinaAppConfigurationEndpointSuffix);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.ContainerRegistryEndpointResourceId, AzureEnvironmentConstants.ChinaContainerRegistryEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureChinaCloud].SetProperty(ExtendedEndpoint.AzureSshAuthScope, AzureEnvironmentConstants.ChinaSshAuthScope);
            }

            if (azureEnvironments.ContainsKey(EnvironmentName.AzureUSGovernment))
            {
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.OperationalInsightsEndpoint, AzureEnvironmentConstants.USGovernmentOperationalInsightsEndpoint);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.OperationalInsightsEndpointResourceId, AzureEnvironmentConstants.USGovernmentOperationalInsightsEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.AnalysisServicesEndpointSuffix, AzureEnvironmentConstants.USGovernmentAnalysisServicesEndpointSuffix);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.AnalysisServicesEndpointResourceId, AzureEnvironmentConstants.USGovernmentAnalysisServicesEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.ManagedHsmServiceEndpointResourceId, AzureEnvironmentConstants.USGovernmeneManagedHsmServiceEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.ManagedHsmServiceEndpointSuffix, AzureEnvironmentConstants.USGovernmentManagedHsmDnsSuffix);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.AzureAppConfigurationEndpointResourceId, AzureEnvironmentConstants.USGovernmentAppConfigurationEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.AzureAppConfigurationEndpointSuffix, AzureEnvironmentConstants.USGovernmentAppConfigurationEndpointSuffix);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.ContainerRegistryEndpointResourceId, AzureEnvironmentConstants.USGovernmentContainerRegistryEndpointResourceId);
                azureEnvironments[EnvironmentName.AzureUSGovernment].SetProperty(ExtendedEndpoint.AzureSshAuthScope, AzureEnvironmentConstants.USGovernmentSshAuthScope);
            }
        } 
    }
}
