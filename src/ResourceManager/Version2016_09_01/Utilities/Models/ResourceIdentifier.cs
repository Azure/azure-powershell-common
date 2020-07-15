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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.Management.Internal.Resources.Utilities.Models
{
    public class ResourceIdentifier
    {
        public string ResourceType { get; set; }

        public string ResourceGroupName { get; set; }

        public string ResourceName { get; set; }

        public string ParentResource { get; set; }

        public string Subscription { get; set; }

        public ResourceIdentifier() { }

        private class UrlTagName
        {
            public const string subscriptions = "subscriptions",
                resourceGroups = "resourceGroups",
                providers = "providers";
        }

        public ResourceIdentifier(string idFromServer)
        {
            if (!string.IsNullOrEmpty(idFromServer))
            {
                string[] tokens = idFromServer.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length % 2 > 0 || tokens.Length < 8)
                {
                    throw new ArgumentException("Invalid format of the resource identifier.", "idFromServer");
                }

                List<string> resourceNameBuilder = new List<string>(), resourceTypeBuilder = new List<string>();
                for (int i = 0; i < tokens.Length; ++i)
                {
                    if (tokens[i].Equals(UrlTagName.subscriptions, StringComparison.OrdinalIgnoreCase))
                    {
                        Subscription = tokens[++i];
                    }
                    else if(tokens[i].Equals(UrlTagName.resourceGroups, StringComparison.OrdinalIgnoreCase))
                    {
                        ResourceGroupName = tokens[++i];
                    }
                    else if(tokens[i].Equals(UrlTagName.providers, StringComparison.OrdinalIgnoreCase))
                    {
                        resourceTypeBuilder.Add(tokens[++i]);
                    }
                    else
                    {
                        resourceTypeBuilder.Add(tokens[i]);
                        resourceNameBuilder.Add(tokens[++i]);
                    }
                }
                List<string> parentResourceBuilder = resourceTypeBuilder.GetRange(1, resourceTypeBuilder.Count - 2);

                if (parentResourceBuilder.Any())
                {
                    parentResourceBuilder.Add(resourceNameBuilder.First());
                    ParentResource = string.Join("/", parentResourceBuilder);
                }
                if (resourceTypeBuilder.Any())
                {
                    ResourceType = string.Join("/", resourceTypeBuilder);
                }
                if(resourceNameBuilder.Any())
                {
                    ResourceName = string.Join("/", resourceNameBuilder);
                }
            }
        }

        public static ResourceIdentifier FromResourceGroupIdentifier(string resourceGroupId)
        {
            if (!string.IsNullOrEmpty(resourceGroupId))
            {
                string[] tokens = resourceGroupId.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 4)
                {
                    throw new ArgumentException("Invalid format of the resource group identifier. Expected 'subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}'.", "resourceGroupId");
                }
                return new ResourceIdentifier
                {
                    Subscription = tokens[1],
                    ResourceGroupName = tokens[3],
                };
            }

            return new ResourceIdentifier();
        }

        public static string GetProviderFromResourceType(string resourceType)
        {
            if (resourceType == null)
            {
                return null;
            }

            int indexOfSlash = resourceType.IndexOf('/');
            if (indexOfSlash < 0)
            {
                return string.Empty;
            }
            else
            {
                return resourceType.Substring(0, indexOfSlash);
            }
        }

        public static string GetTypeFromResourceType(string resourceType)
        {
            if (resourceType == null)
            {
                return null;
            }

            int lastIndexOfSlash = resourceType.LastIndexOf('/');
            if (lastIndexOfSlash < 0)
            {
                return string.Empty;
            }
            else
            {
                return resourceType.Substring(lastIndexOfSlash + 1);
            }
        }

        public override string ToString()
        {
            string provider = GetProviderFromResourceType(ResourceType);
            string type = GetTypeFromResourceType(ResourceType);
            string parentAndType = string.IsNullOrEmpty(ParentResource) ? type : ParentResource + "/" + type;
            StringBuilder resourceId = new StringBuilder();

            AppendIfNotNull(resourceId, "/subscriptions/{0}", Subscription);
            AppendIfNotNull(resourceId, "/resourceGroups/{0}", ResourceGroupName);
            AppendIfNotNull(resourceId, "/providers/{0}", provider);
            AppendIfNotNull(resourceId, "/{0}", parentAndType);
            string subResourceName = ResourceName;
            int lastIndexOfSlash = ResourceName.LastIndexOf('/');
            if (lastIndexOfSlash > 0)
            {
                subResourceName = ResourceName.Substring(lastIndexOfSlash + 1);
            }
            AppendIfNotNull(resourceId, "/{0}", subResourceName);

            return resourceId.ToString();
        }

        private void AppendIfNotNull(StringBuilder resourceId, string format, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                resourceId.AppendFormat(format, value);
            }
        }
    }
}
