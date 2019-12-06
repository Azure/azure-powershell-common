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
using Newtonsoft.Json;
using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
using System.Text;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System.Collections;
using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using System;
using System.Linq;

namespace Microsoft.Azure.Management.Internal.Resources.Utilities
{
    /// <summary>
    /// Extension methods for ResourceManager cmdlets
    /// </summary>
    public static class ResourcesExtensionsTemplate
    {
        /// <summary>
        /// Add resource group metadata to a deployment result
        /// </summary>
        /// <param name="result">The result to augment</param>
        /// <param name="resourceGroup">Metadata on the resource group for the deployment</param>
        /// <returns>A ResourceGroupDeployment object combining the metadata about the deployment and the resource group it is deployed to</returns>
        public static ResourceGroupDeployment<TMode, TLink> ToPSResourceGroupDeployment<TDeployment, TMode, TLink>(this TDeployment result, string resourceGroup) where TMode : struct, IConvertible
        {
            var deployment = new ResourceGroupDeployment<TMode, TLink>();

            if (result != null)
            {
                var type = result.GetType().GetProperty("Properties").GetValue(result, null).GetType();
                deployment = CreatePSResourceGroupDeployment<TMode, TLink, Object> (
                    result.GetType().GetProperty("Name").GetValue(result, null) as string
                    ,resourceGroup
                    ,result.GetType().GetProperty("Properties").GetValue(result, null));
            }

            return deployment;
        }

        /// <summary>
        /// Create a text formatted table for deployment variable values
        /// </summary>
        /// <param name="dictionary">Key value pairs for each of the deployment variables</param>
        /// <returns>A string representing the given deployment variables in tabular form</returns>
        public static string ConstructDeploymentVariableTable(Dictionary<string, DeploymentVariable> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            StringBuilder result = new StringBuilder();

            if (dictionary.Count > 0)
            {
                string rowFormat = "{0, -15}  {1, -25}  {2, -10}\r\n";
                result.AppendLine();
                result.AppendFormat(rowFormat, "Name", "Type", "Value");
                result.AppendFormat(rowFormat, GeneralUtilities.GenerateSeparator(15, "="), GeneralUtilities.GenerateSeparator(25, "="), GeneralUtilities.GenerateSeparator(10, "="));

                foreach (KeyValuePair<string, DeploymentVariable> pair in dictionary)
                {
                    result.AppendFormat(rowFormat, pair.Key, pair.Value.Type, pair.Value.Value);
                }
            }

            return result.ToString();

        }

        /// <summary>
        /// Create a tabular format for resource tags metadata
        /// </summary>
        /// <param name="tags">The dictionary of tags</param>
        /// <returns>A string representation of the tags formatted as a table</returns>
        public static string ConstructTagsTable(Hashtable tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return null;
            }

            StringBuilder resourcesTable = new StringBuilder();

            var tagsDictionary = TagsConversionHelper.CreateTagDictionary(tags, false);

            int maxNameLength = Math.Max("Name".Length, tagsDictionary.Max(tag => tag.Key.Length));
            int maxValueLength = Math.Max("Value".Length, tagsDictionary.Max(tag => tag.Value.Length));

            string rowFormat = "{0, -" + maxNameLength + "}  {1, -" + maxValueLength + "}\r\n";
            resourcesTable.AppendLine();
            resourcesTable.AppendFormat(rowFormat, "Name", "Value");
            resourcesTable.AppendFormat(rowFormat,
                GeneralUtilities.GenerateSeparator(maxNameLength, "="),
                GeneralUtilities.GenerateSeparator(maxValueLength, "="));

            foreach (var tag in tagsDictionary)
            {
                if (tag.Key.StartsWith(TagsClient.ExecludedTagPrefix))
                {
                    continue;
                }

                resourcesTable.AppendFormat(rowFormat, tag.Key, tag.Value);
            }

            return resourcesTable.ToString();
        }

        static TMode? getMode<TDeploymentPropertiesExtended, TMode>(TDeploymentPropertiesExtended properties) where TMode : struct, IConvertible
        {
            return (TMode) properties.GetType().GetProperty("Mode").GetValue(properties, null);
        }

        static string getProvisioningState<TDeploymentPropertiesExtended>(TDeploymentPropertiesExtended properties)
        {
            return properties.GetType().GetProperty("ProvisioningState").GetValue(properties, null) as string;
        }

        static TLink getTemplateLink<TDeploymentPropertiesExtended, TLink>(TDeploymentPropertiesExtended properties)
        {
            return (TLink) properties.GetType().GetProperty("TemplateLink").GetValue(properties, null);
        }

        static DateTime getTimeStampe<TDeploymentPropertiesExtended>(TDeploymentPropertiesExtended properties)
        {
            return (DateTime) properties.GetType().GetProperty("Timestamp").GetValue(properties, null);
        }

        static string getCorrelationId<TDeploymentPropertiesExtended>(TDeploymentPropertiesExtended properties)
        {
            return properties.GetType().GetProperty("CorrelationId").GetValue(properties, null) as string;
        }

        static object getDebugSetting<TDeploymentPropertiesExtended>(TDeploymentPropertiesExtended properties)
        {
            return properties.GetType().GetProperty("DebugSetting").GetValue(properties, null);
        }

        static string getDebugSettingDetailLevel<TDeploymentPropertiesExtended>(TDeploymentPropertiesExtended properties)
        {
            return getDebugSetting(properties).GetType().GetProperty("DebugSetting").GetValue(properties, null) as string;
        }

        static Dictionary<string, DeploymentVariable> getOutputs<TDeploymentPropertiesExtended>(TDeploymentPropertiesExtended properties)
        {
            return properties.GetType().GetProperty("Outputs").GetValue(properties, null) as Dictionary<string, DeploymentVariable>;
        }

        static Dictionary<string, DeploymentVariable> getParameters<TDeploymentPropertiesExtended>(TDeploymentPropertiesExtended properties)
        {
            return properties.GetType().GetProperty("Parameters").GetValue(properties, null) as Dictionary<string, DeploymentVariable>;
        }

        private static ResourceGroupDeployment<TMode, TLink> CreatePSResourceGroupDeployment<TMode, TLink, TDeploymentPropertiesExtended>(
            string name,
            string gesourceGroup,
            TDeploymentPropertiesExtended properties) where TMode : struct, IConvertible
        {
            var deploymentObject = new ResourceGroupDeployment<TMode, TLink>();

            deploymentObject.DeploymentName = name;
            deploymentObject.ResourceGroupName = gesourceGroup;

            if (properties != null)
            {
                deploymentObject.Mode =  getMode<TDeploymentPropertiesExtended, TMode>(properties);
                deploymentObject.ProvisioningState = getProvisioningState(properties);
                TLink templateLink = getTemplateLink<TDeploymentPropertiesExtended, TLink>(properties);
                deploymentObject.TemplateLink = templateLink;
                deploymentObject.Timestamp = getTimeStampe(properties);
                deploymentObject.CorrelationId = getCorrelationId(properties);

                if (getDebugSetting<TDeploymentPropertiesExtended>(properties) != null && !string.IsNullOrEmpty(getDebugSettingDetailLevel(properties)))
                {
                    deploymentObject.DeploymentDebugLogLevel = getDebugSettingDetailLevel(properties);
                }

                if (getOutputs(properties) != null && !string.IsNullOrEmpty(getOutputs(properties).ToString()))
                {
                    Dictionary<string, DeploymentVariable> outputs = JsonConvert.DeserializeObject<Dictionary<string, DeploymentVariable>>(getOutputs(properties).ToString());
                    deploymentObject.Outputs = outputs;
                }

                if (getParameters(properties) != null && !string.IsNullOrEmpty(getParameters(properties).ToString()))
                {
                    Dictionary<string, DeploymentVariable> parameters = JsonConvert.DeserializeObject<Dictionary<string, DeploymentVariable>>(getParameters(properties).ToString());
                    deploymentObject.Parameters = parameters;
                }

                if (templateLink != null)
                {
                    deploymentObject.TemplateLinkString = ConstructTemplateLinkView<TLink>(templateLink);
                }
            }

            return deploymentObject;
        }

        private static string ConstructTemplateLinkView<TLink>(TLink templateLink)
        {
            if (templateLink == null)
            {
                return string.Empty;
            }

            StringBuilder result = new StringBuilder();

            result.AppendLine();
            result.AppendLine(string.Format("{0, -15}: {1}", "Uri", templateLink.GetType().GetProperty("Uri").GetValue(templateLink)));
            result.AppendLine(string.Format("{0, -15}: {1}", "ContentVersion", templateLink.GetType().GetProperty("ContentVersion").GetValue(templateLink)));

            return result.ToString();
        }
    }
}