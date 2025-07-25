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

using System;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// Known endpoint values for public azure clouds
    /// </summary>
    public static class AzureEnvironmentConstants
    {
        /// <summary>
        /// The default AD Tenant value
        /// </summary>
        public const string CommonAdTenant = "Common";

        /// <summary>
        /// RDFE endpoints
        /// </summary>
        public const string AzureServiceEndpoint = "https://management.core.windows.net/";
        public const string ChinaServiceEndpoint = "https://management.core.chinacloudapi.cn/";
        public const string USGovernmentServiceEndpoint = "https://management.core.usgovcloudapi.net/";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanServiceEndpoint = "https://management.core.cloudapi.de/";

        /// <summary>
        /// Azure ResourceManager endpoints
        /// </summary>
        public const string AzureResourceManagerEndpoint = "https://management.azure.com/";
        public const string ChinaResourceManagerEndpoint = "https://management.chinacloudapi.cn/";
        public const string USGovernmentResourceManagerEndpoint = "https://management.usgovcloudapi.net/";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanResourceManagerEndpoint = "https://management.microsoftazure.de/";

        /// <summary>
        /// Template gallery endpoints
        /// </summary>
        public const string GalleryEndpoint = "https://gallery.azure.com/";
        public const string ChinaGalleryEndpoint = "https://gallery.chinacloudapi.cn/";
        public const string USGovernmentGalleryEndpoint = "https://gallery.usgovcloudapi.net/";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanGalleryEndpoint = "https://gallery.cloudapi.de/";

        /// <summary>
        /// Location of the publishsettings file web application
        /// </summary>
        public const string AzurePublishSettingsFileUrl = "https://go.microsoft.com/fwlink/?LinkID=301775";
        public const string ChinaPublishSettingsFileUrl = "https://go.microsoft.com/fwlink/?LinkID=301776";
        public const string USGovernmentPublishSettingsFileUrl = "https://manage.windowsazure.us/publishsettings/index";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanPublishSettingsFileUrl = "https://manage.microsoftazure.de/publishsettings/index";

        /// <summary>
        /// Location of the maagement portal for the environment
        /// </summary>
        public const string AzureManagementPortalUrl = "https://portal.azure.com/";
        public const string ChinaManagementPortalUrl = "https://portal.azure.cn/";
        public const string USGovernmentManagementPortalUrl = "https://portal.azure.us/";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanManagementPortalUrl = "https://portal.microsoftazure.de/";

        /// <summary>
        /// The domain name suffix for storage services
        /// </summary>
        public const string AzureStorageEndpointSuffix = "core.windows.net";
        public const string ChinaStorageEndpointSuffix = "core.chinacloudapi.cn";
        public const string USGovernmentStorageEndpointSuffix = "core.usgovcloudapi.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanStorageEndpointSuffix = "core.cloudapi.de";

        /// <summary>
        /// The domain name suffix for Sql database services
        /// </summary>
        public const string AzureSqlDatabaseDnsSuffix = ".database.windows.net";
        public const string ChinaSqlDatabaseDnsSuffix = ".database.chinacloudapi.cn";
        public const string USGovernmentSqlDatabaseDnsSuffix = ".database.usgovcloudapi.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanSqlDatabaseDnsSuffix = ".database.cloudapi.de";

        /// <summary>
        /// The Azure Active Directory authentication endpoint
        /// </summary>
        public const string AzureActiveDirectoryEndpoint = "https://login.microsoftonline.com/";
        public const string ChinaActiveDirectoryEndpoint = "https://login.chinacloudapi.cn/";
        public const string USGovernmentActiveDirectoryEndpoint = "https://login.microsoftonline.us/";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanActiveDirectoryEndpoint = "https://login.microsoftonline.de/";

        /// <summary>
        /// The Azure Active Directory Graph endpoint
        /// </summary>
        public const string AzureGraphEndpoint = "https://graph.windows.net/";
        public const string ChinaGraphEndpoint = "https://graph.chinacloudapi.cn/";
        public const string USGovernmentGraphEndpoint = "https://graph.microsoftazure.us/";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanGraphEndpoint = "https://graph.cloudapi.de/";

        /// <summary>
        /// The domian name suffix for Traffic manager services
        /// </summary>
        public const string AzureTrafficManagerDnsSuffix = "trafficmanager.net";
        public const string ChinaTrafficManagerDnsSuffix = "trafficmanager.cn";
        public const string USGovernmentTrafficManagerDnsSuffix = "usgovtrafficmanager.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanTrafficManagerDnsSuffix = "azuretrafficmanager.de";

        /// <summary>
        /// The domain name suffix for azure keyvault vaults
        /// </summary>
        public const string AzureKeyVaultDnsSuffix = "vault.azure.net";
        public const string ChinaKeyVaultDnsSuffix = "vault.azure.cn";
        public const string USGovernmentKeyVaultDnsSuffix = "vault.usgovcloudapi.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanKeyVaultDnsSuffix = "vault.microsoftazure.de";

        /// <summary>
        /// The domain name suffix for azure keyvault managed hsms
        /// </summary>
        public const string AzureManagedHsmDnsSuffix = "managedhsm.azure.net";
        public const string ChinaManagedHsmDnsSuffix = "managedhsm.azure.cn";
        public const string USGovernmentManagedHsmDnsSuffix = "managedhsm.usgovcloudapi.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanManagedHsmDnsSuffix = "managedhsm.microsoftazure.de";

        /// <summary>
        /// The token audience for authorizing KeyVault requests
        /// </summary>
        public const string AzureKeyVaultServiceEndpointResourceId = "https://vault.azure.net";
        public const string ChinaKeyVaultServiceEndpointResourceId = "https://vault.azure.cn";
        public const string USGovernmentKeyVaultServiceEndpointResourceId = "https://vault.usgovcloudapi.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanAzureKeyVaultServiceEndpointResourceId = "https://vault.microsoftazure.de";

        /// <summary>
        /// The token audience for authorizing managed hsm requests
        /// </summary>
        public const string AzureManagedHsmServiceEndpointResourceId = "https://managedhsm.azure.net";
        public const string ChinaManagedHsmServiceEndpointResourceId = "https://managedhsm.azure.cn";
        public const string USGovernmeneManagedHsmServiceEndpointResourceId = "https://managedhsm.usgovcloudapi.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanAzureManagedHsmServiceEndpointResourceId = "https://managedhsm.microsoftazure.de";

        /// <summary>
        /// The token audience for Log Analytics Queries
        /// </summary>
        public const string AzureOperationalInsightsEndpointResourceId = "https://api.loganalytics.io";
        public const string ChinaOperationalInsightsEndpointResourceId = "https://api.loganalytics.azure.cn";
        public const string USGovernmentOperationalInsightsEndpointResourceId = "https://api.loganalytics.us";

        /// <summary>
        /// The endpoint URI for Log Analytics Queries
        /// </summary>
        public const string AzureOperationalInsightsEndpoint = "https://api.loganalytics.io/v1";
        public const string ChinaOperationalInsightsEndpoint = "https://api.loganalytics.azure.cn/v1";
        public const string USGovernmentOperationalInsightsEndpoint = "https://api.loganalytics.us/v1";

        /// <summary>
        /// The domain name suffix for Azure DataLake services
        /// </summary>
        public const string AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = "azuredatalakeanalytics.net";
        public const string AzureDataLakeStoreFileSystemEndpointSuffix = "azuredatalakestore.net";

        /// <summary>
        /// The token audience for authorizing DataLake requests
        /// </summary>
        public const string AzureDataLakeServiceEndpointResourceId = "https://datalake.azure.net/";

        /// <summary>
        /// The token audience for Batch data plane requests
        /// </summary>
        public const string BatchEndpointResourceId = "https://batch.core.windows.net/";
        public const string ChinaBatchEndpointResourceId = "https://batch.chinacloudapi.cn/";
        public const string USGovernmentBatchEndpointResourceId = "https://batch.core.usgovcloudapi.net/";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanBatchEndpointResourceId = "https://batch.cloudapi.de/";

        /// <summary>
        /// The domain name suffix for Azure Analysis Services
        /// </summary>
        public const string AzureAnalysisServicesEndpointSuffix = "asazure.windows.net";
        public const string ChinaAnalysisServicesEndpointSuffix = "asazure.chinacloudapi.cn";
        public const string USGovernmentAnalysisServicesEndpointSuffix = "asazure.usgovcloudapi.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanAnalysisServicesEndpointSuffix = "asazure.cloudapi.de";

        /// <summary>
        /// The token audiences for authorizing Analysis Service requests
        /// </summary>
        /// <remarks>
        /// Analysis Service expects a token audience which matches "https://*.asazure.windows.net".
        /// The wildcard takes place of the region, however the region cannot be calculated here.
        /// "region" can take place of the region, since "*" is an invalid character for URIs.
        /// </remarks>
        public const string AzureAnalysisServicesEndpointResourceId = "https://region.asazure.windows.net";
        public const string ChinaAnalysisServicesEndpointResourceId = "https://region.asazure.chinacloudapi.cn";
        public const string USGovernmentAnalysisServicesEndpointResourceId = "https://region.asazure.usgovcloudapi.net";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanAnalysisServicesEndpointResourceId = "https://region.asazure.cloudapi.de";

        /// <summary>
        /// The domain name suffix for Azure Attestation Services
        /// </summary>
        public const string AzureAttestationServiceEndpointSuffix = "attest.azure.net";

        /// <summary>
        /// The token audience for authorizing Attestation Service requests
        /// </summary>
        public const string AzureAttestationServiceEndpointResourceId = "https://attest.azure.net";

        /// <summary>
        /// The domain name suffix for Azure Synapse Services
        /// </summary>
        public const string AzureSynapseAnalyticsEndpointSuffix = "dev.azuresynapse.net";
        public const string ChinaSynapseAnalyticsEndpointSuffix = "dev.azuresynapse.azure.cn";
        public const string USGovernmentSynapseAnalyticsEndpointSuffix = "dev.azuresynapse.usgovcloudapi.net";

        /// <summary>
        /// The token audience for authorizing Synapse Service requests
        /// </summary>
        public const string AzureSynapseAnalyticsEndpointResourceId = "https://dev.azuresynapse.net";
        public const string ChinaSynapseAnalyticsEndpointResourceId = "https://dev.azuresynapse.azure.cn";
        public const string USGovernmentSynapseAnalyticsEndpointResourceId = "https://dev.azuresynapse.usgovcloudapi.net";

        /// <summary>
        /// The domain name suffix for Azure Container Registry
        /// </summary>
        public const string AzureContainerRegistryEndpointSuffix = "azurecr.io";
        public const string ChinaContainerRegistryEndpointSuffix = "azurecr.cn";
        public const string USGovernmentContainerRegistryEndpointSuffix = "azurecr.us";

        /// <summary>
        /// MSGraph endpoints.
        /// </summary>
        public const string AzureMicrosoftGraphUrl = "https://graph.microsoft.com";
        public const string ChinaMicrosoftGraphUrl = "https://microsoftgraph.chinacloudapi.cn";
        public const string USGovernmentMicrosoftGraphUrl = "https://graph.microsoft.us";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanMicrosoftGraphUrl = "https://graph.microsoft.de";

        /// <summary>
        /// MSGraph Resource Url
        /// </summary>
        public const string AzureMicrosoftGraphEndpointResourceId = "https://graph.microsoft.com/";
        public const string ChinaMicrosoftGraphEndpointResourceId = "https://microsoftgraph.chinacloudapi.cn/";
        public const string USGovernmentMicrosoftGraphEndpointResourceId = "https://graph.microsoft.us/";
        [Obsolete("Microsoft Cloud Germany was closed on October 29th, 2021.")]
        public const string GermanMicrosoftGraphEndpointResourceId = "https://graph.microsoft.de/";

        /// <summary>
        /// The domain name suffix for Azure Purview Services
        /// </summary>
        public const string AzurePurviewEndpointSuffix = "purview.azure.net";

        /// <summary>
        /// The token audience for authorizing Purview Service requests
        /// </summary>
        public const string AzurePurviewEndpointResourceId = "https://purview.azure.net";

        /// <summary>
        /// The domain name suffix for App Configuration
        /// </summary>
        public const string AzureAppConfigurationEndpointSuffix = "azconfig.io";
        public const string ChinaAppConfigurationEndpointSuffix = "azconfig.azure.cn";
        public const string USGovernmentAppConfigurationEndpointSuffix = "azconfig.azure.us";

        /// <summary>
        /// The endpoint Resource Id for App Configuration
        /// </summary>
        public const string AzureAppConfigurationEndpointResourceId = "https://azconfig.io";
        public const string ChinaAppConfigurationEndpointResourceId = "https://azconfig.azure.cn";
        public const string USGovernmentAppConfigurationEndpointResourceId = "https://azconfig.azure.us";

        /// <summary>
        /// The endpoint Resource Id for Azure Container Registry
        /// </summary>
        public const string ChinaContainerRegistryEndpointResourceId = "https://management.chinacloudapi.cn";
        public const string USGovernmentContainerRegistryEndpointResourceId = "https://management.usgovcloudapi.net";
        public const string AzureContainerRegistryEndpointResourceId = "https://management.azure.com";

        /// <summary>
        /// Communication Email
        /// </summary>
        public const string AzureCommunicationEmailEndpointSuffix = "communication.azure.com";
        public const string AzureCommunicationEmailEndpointResourceId = "https://communication.azure.com";

        /// <summary>
        /// The scope for SSH authentication. See <see cref="SshCredentialFactory"/>
        /// </summary>
        public const string AzureSshAuthScope = "https://pas.windows.net/CheckMyAccess/Linux/.default";
        public const string ChinaSshAuthScope = "https://pas.chinacloudapi.cn/CheckMyAccess/Linux/.default";
        public const string USGovernmentSshAuthScope = "https://pasff.usgovcloudapi.net/CheckMyAccess/Linux/.default";
    }
}
