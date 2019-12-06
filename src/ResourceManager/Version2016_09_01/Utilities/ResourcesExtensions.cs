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

using Microsoft.Azure.Management.Internal.Resources.Models;
using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.Internal.Resources.Utilities
{
    /// <summary>
    /// Extension methods for ResourceManager cmdlets
    /// </summary>
    public static class ResourcesExtensions
    {
        /// <summary>
        /// Add resource group metadata to a deployment result
        /// </summary>
        /// <param name="result">The result to augment</param>
        /// <param name="resourceGroup">Metadata on the resource group for the deployment</param>
        /// <returns>A ResourceGroupDeployment object combining the metadata about the deployment and the resource group it is deployed to</returns>
        public static ResourceGroupDeployment<DeploymentMode, TemplateLink> ToPSResourceGroupDeployment(this DeploymentExtended result, string resourceGroup)
        {
            return ResourcesExtensionsTemplate.ToPSResourceGroupDeployment<DeploymentExtended, DeploymentMode, TemplateLink>(result, resourceGroup);
        }

        /// <summary>
        /// Create a text formatted table for deployment variable values
        /// </summary>
        /// <param name="dictionary">Key value pairs for each of the deployment variables</param>
        /// <returns>A string representing the given deployment variables in tabular form</returns>
        public static string ConstructDeploymentVariableTable(Dictionary<string, DeploymentVariable> dictionary)
        {
            return ResourcesExtensionsTemplate.ConstructDeploymentVariableTable(dictionary);
        }

        /// <summary>
        /// Create a tabular format for resource tags metadata
        /// </summary>
        /// <param name="tags">The dictionary of tags</param>
        /// <returns>A string representation of the tags formatted as a table</returns>
        public static string ConstructTagsTable(Hashtable tags)
        {
            return ResourcesExtensionsTemplate.ConstructTagsTable(tags);
        }
    }
}