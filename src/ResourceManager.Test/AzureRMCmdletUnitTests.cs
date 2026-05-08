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

using Microsoft.Azure.Commands.ResourceManager.Common;
using Microsoft.WindowsAzure.Commands.Common.Attributes;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Test
{
    public class AzureRMCmdletUnitTests
    {
        [Fact]
        public void HasAttributeHasSubId()
        {
            var positive = new PositiveCmdlet();
            var dynamicParameters = positive.GetDynamicParameters() as RuntimeDefinedParameterDictionary;
            Assert.NotNull(dynamicParameters);
            // AcquirePolicyToken + ChangeReference + SubscriptionId = 3
            Assert.Equal(3, dynamicParameters.Count);
            Assert.NotNull(dynamicParameters["SubscriptionId"]);
            Assert.NotNull(dynamicParameters["AcquirePolicyToken"]);
            Assert.NotNull(dynamicParameters["ChangeReference"]);
        }

        [Fact]
        public void NoAttributeNoSubId()
        {
            var negative = new NegativeCmdlet();
            var dynamicParameters = negative.GetDynamicParameters() as RuntimeDefinedParameterDictionary;
            // AcquirePolicyToken + ChangeReference = 2 (no SubscriptionId since no [SupportsSubscriptionId])
            Assert.Equal(2, dynamicParameters.Count);
            Assert.NotNull(dynamicParameters["AcquirePolicyToken"]);
            Assert.NotNull(dynamicParameters["ChangeReference"]);
        }

        [Fact]
        public void DoubleDynamicParameters()
        {
            // when child cmdlet also has dynamic parameter
            // it should combine with base class
            var positiveWithOwnParam = new PositiveCmdletWithOwnDynamicParam();
            var dynamicParameters = positiveWithOwnParam.GetDynamicParameters() as RuntimeDefinedParameterDictionary;
            Assert.NotNull(dynamicParameters);
            // AcquirePolicyToken + ChangeReference + SubscriptionId + DP = 4
            Assert.Equal(4, dynamicParameters.Count);
            Assert.NotNull(dynamicParameters["AcquirePolicyToken"]);
            Assert.NotNull(dynamicParameters["ChangeReference"]);
            Assert.NotNull(dynamicParameters["SubscriptionId"]);
            Assert.NotNull(dynamicParameters["DP"]);
        }

        [SupportsSubscriptionId]
        class PositiveCmdlet : AzureRMCmdlet
        {
        }

        class NegativeCmdlet : AzureRMCmdlet { }

        [SupportsSubscriptionId]
        class PositiveCmdletWithOwnDynamicParam : AzureRMCmdlet, IDynamicParameters
        {
            public new object GetDynamicParameters()
            {
                var parameters = base.GetDynamicParameters() as RuntimeDefinedParameterDictionary;
                parameters.Add("DP", new RuntimeDefinedParameter(
                    "DP",
                    typeof(string),
                    new Collection<Attribute>()
                    {
                        new ParameterAttribute { }
                    }
                ));
                return parameters;
            }
        }
    }
}
