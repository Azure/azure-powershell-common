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

using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
using Microsoft.Rest.Azure;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.Internal.Resources.Utilities
{
    public static class ResourceManagementClientExtensionsTemplate
    {
        public static List<TResource> FilterResources<TClient, TResource, TFilter>(this TClient client, FilterResourcesOptions options)
        {
            List<TResource> resources = new List<TResource>();
            var iresources = typeof(TClient).GetProperty("Resources").GetValue(client);
            var iresourcegroup = typeof(TClient).GetProperty("ResourceGroups").GetValue(client);

            if (!string.IsNullOrEmpty(options.ResourceGroup) && !string.IsNullOrEmpty(options.Name))
            {
                resources.Add((TResource)iresources.GetType().GetMethod("Get").Invoke(iresources, new [] {options.ResourceGroup, null, null, null, options.Name, null}));
            }
            else
            {
                if (!string.IsNullOrEmpty(options.ResourceGroup))
                {
                    Rest.Azure.IPage<TResource> result = iresourcegroup.GetType().GetMethod("ListResources").Invoke(iresourcegroup, new[]{options.ResourceGroup,
                        new Rest.Azure.OData.ODataQuery<TResource>(r => (r.GetType().GetProperty("ResourceType").GetValue(r) as string) == options.ResourceType)}) as IPage<TResource>;

                    resources.AddRange(result);
                    while (!string.IsNullOrEmpty(result.NextPageLink))
                    {
                        result = iresourcegroup.GetType().GetMethod("ListResources").Invoke(iresourcegroup, new[] { result.NextPageLink }) as IPage<TResource>;
                        resources.AddRange(result);
                    }
                }
                else
                {
                    Rest.Azure.IPage<TResource> result = (IPage<TResource>)iresources.GetType().GetMethod("List").Invoke(iresources, new[] { new Rest.Azure.OData.ODataQuery<TFilter>(r => (r.GetType().GetProperty("ResourceType").GetValue(r) as string) == options.ResourceType) });

                    resources.AddRange(result);
                    while (!string.IsNullOrEmpty(result.NextPageLink))
                    {
                        result = (IPage<TResource>)iresources.GetType().GetMethod("List").Invoke(iresources, new[] { result.NextPageLink });
                        resources.AddRange(result);
                    }
                }
            }

            foreach (var resource in resources)
            {
                var identifier = new ResourceIdentifier(resource.GetType().GetProperty("Id").GetValue(resource) as string);
                resource.GetType().GetProperty("ResourceGroupName").SetValue(resource, identifier.ResourceGroupName);
            }

            return resources;
        }
    }
}
