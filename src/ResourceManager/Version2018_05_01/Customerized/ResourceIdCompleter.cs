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
using Microsoft.Azure.Management.Internal.ResourceManager.Version2018_05_01;
using Microsoft.Azure.Management.Internal.ResourceManager.Version2018_05_01.Models;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters.Version2018_05_01
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ResourceIdCompleterAttribute : ResourceIdCompleterAttributeTemplate
    {
        /// <summary>
        /// Consturctor
        /// </summary>
        /// <param name="resourceType">Azure recource type</param>
        public ResourceIdCompleterAttribute(string resourceType)
            : base(resourceType)
        {}

        public static IEnumerable<string> GetResourceIds(string resourceType)
        {
            return GetResourceIds<ResourceManagementClient, GenericResourceFilter>(resourceType, r => r.ResourceType == resourceType);
        }
    }
}
