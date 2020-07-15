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
using Xunit;

namespace Microsoft.Azure.Management.Internal.Resources.Utilities.Models.Test
{
    public class ResourceIdentifierUnitTest
    {
        [Fact]
        public void FromResourceIdentifierCommon()
        {
            string subscriptionId = Guid.NewGuid().ToString(),
                resourceGroup = "TestGroup",
                serverName = "TestServer";
            string id = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.DBforPostgreSQL/servers/{serverName}";
            ResourceIdentifier identifier = new ResourceIdentifier(id);
            Assert.Equal($"{subscriptionId}", identifier.Subscription);
            Assert.Null(identifier.ParentResource);
            Assert.Equal("Microsoft.DBforPostgreSQL/servers", identifier.ResourceType);
            Assert.Equal($"{resourceGroup}", identifier.ResourceGroupName);
            Assert.Equal($"{serverName}", identifier.ResourceName);
            Assert.Equal(id, identifier.ToString());
        }

        [Fact]
        public void FromResourceIdentifierWithSubResource()
        {
            string subscriptionId = Guid.NewGuid().ToString(),
                resourceGroup = "TestGroup",
                serverName = "TestServer";
            string id = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.DBforPostgreSQL/servers/{serverName}/configurations/datestyle";
            ResourceIdentifier identifier = new ResourceIdentifier(id);
            Assert.Equal($"{subscriptionId}", identifier.Subscription);
            Assert.Equal($"servers/{serverName}", identifier.ParentResource);
            Assert.Equal("Microsoft.DBforPostgreSQL/servers/configurations", identifier.ResourceType);
            Assert.Equal($"{resourceGroup}", identifier.ResourceGroupName);
            Assert.Equal($"{serverName}/datestyle", identifier.ResourceName);
            Assert.Equal(id, identifier.ToString());
        }

        [Fact]
        public void FromInvalidResourceIdentifier()
        {
            string subscriptionId = Guid.NewGuid().ToString(),
                resourceGroup = "TestGroup",
                serverName = "TestServer";
            string id = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.DBforPostgreSQL/servers/{serverName}/configurations";
            Assert.Throws< ArgumentException>( () =>  new ResourceIdentifier(id));

            id = $"/subscriptions/${subscriptionId}/resourceGroups/${resourceGroup}/providers/Microsoft.DBforPostgreSQL/servers";
            Assert.Throws<ArgumentException>(() => new ResourceIdentifier(id));
        }
    }
}
