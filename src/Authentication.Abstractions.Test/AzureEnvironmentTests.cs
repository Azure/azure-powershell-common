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

using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using System;
using Xunit;

namespace Authentication.Abstractions.Test
{
    public class AzureEnvironmentTests
    {
        private const string ArmMetadataEnvVariable = "ARM_CLOUD_METADATA_URL";

        [Fact]
        public void TestArmAndNonArmBasedCloudMetadataInit()
        {
            Environment.SetEnvironmentVariable(ArmMetadataEnvVariable, @"TestData/GoodArmResponse.json");
            var armEnvironments = AzureEnvironment.InitializeBuiltInEnvironments(null, httpOperations: TestOperationsFactory.Create().GetHttpOperations());

            // Check all discovered environments are loaded.
            Assert.Equal(4, armEnvironments.Count);
            foreach (var env in armEnvironments.Values)
            {
                Assert.Equal(AzureEnvironment.TypeDiscovered, env.Type);
            }
        }

        [Fact]
        public void TestArmCloudMetadata20190501Init()
        {
            Environment.SetEnvironmentVariable(ArmMetadataEnvVariable, @"TestData/ArmResponse2019-05-01.json");
            var armEnvironments = AzureEnvironment.InitializeBuiltInEnvironments(null, httpOperations: TestOperationsFactory.Create().GetHttpOperations());

            // Check all discovered environments are loaded.
            Assert.Equal(4, armEnvironments.Count);
            foreach (var env in armEnvironments.Values)
            {
                Assert.Equal(AzureEnvironment.TypeDiscovered, env.Type);
                Assert.EndsWith("/", env.ServiceManagementUrl);
                Assert.StartsWith(".", env.SqlDatabaseDnsSuffix);
            }
        }

        [Fact]
        public void TestArmCloudMetadata20220901Init()
        {
            Environment.SetEnvironmentVariable(ArmMetadataEnvVariable, @"TestData/ArmResponse2022-09-01.json");
            var armEnvironments = AzureEnvironment.InitializeBuiltInEnvironments(null, httpOperations: TestOperationsFactory.Create().GetHttpOperations());

            // Check all discovered environments are loaded.
            Assert.Equal(3, armEnvironments.Count);
            foreach (var env in armEnvironments.Values)
            {
                if (env.Name == EnvironmentName.AzureCloud)
                {
                    Assert.Equal(AzureEnvironment.TypeDiscovered, env.Type);
                    Assert.Null(env.GalleryUrl);
                }
                else
                {
                    Assert.Equal(AzureEnvironment.TypeBuiltIn, env.Type);
                    Assert.NotEmpty(env.GalleryUrl);
                }
                Assert.EndsWith("/", env.ServiceManagementUrl);
                Assert.StartsWith(".", env.SqlDatabaseDnsSuffix);
            }
        }

        [Fact]
        public void TestArmResponseNoAzureCloud()
        {
            Environment.SetEnvironmentVariable(ArmMetadataEnvVariable, @"TestData/ArmResponseNoAzureCloud.json");
            var armEnvironments = AzureEnvironment.InitializeBuiltInEnvironments(null, httpOperations: TestOperationsFactory.Create().GetHttpOperations());

            // Check AzureCloud is added to public environment list even discovery endpoint doesn't return AzureCloud.
            Assert.Equal(3, armEnvironments.Count);
            Assert.Equal(AzureEnvironment.TypeBuiltIn, armEnvironments[EnvironmentName.AzureCloud].Type);
            Assert.Equal(AzureEnvironment.TypeDiscovered, armEnvironments[EnvironmentName.AzureChinaCloud].Type);
            Assert.Equal(AzureEnvironment.TypeDiscovered, armEnvironments[EnvironmentName.AzureUSGovernment].Type);
        }

        [Fact]
        public void TestArmResponseOneEntry()
        {
            Environment.SetEnvironmentVariable(ArmMetadataEnvVariable, @"TestData/ArmResponseOneEntry.json");
            var armEnvironments = AzureEnvironment.InitializeBuiltInEnvironments(null, httpOperations: TestOperationsFactory.Create().GetHttpOperations());

            Assert.Equal(4, armEnvironments.Count);
            Assert.Equal(AzureEnvironment.TypeBuiltIn, armEnvironments[EnvironmentName.AzureCloud].Type);
            Assert.Equal(AzureEnvironment.TypeBuiltIn, armEnvironments[EnvironmentName.AzureChinaCloud].Type);
            Assert.Equal(AzureEnvironment.TypeBuiltIn, armEnvironments[EnvironmentName.AzureUSGovernment].Type);
        }

        [Fact]
        public void TestFallbackWhenArmCloudMetadataInitFails()
        {
            Environment.SetEnvironmentVariable(ArmMetadataEnvVariable, @"TestData\BadArmResponse.json");
            var armEnvironments = AzureEnvironment.InitializeBuiltInEnvironments(null, httpOperations: TestOperationsFactory.Create().GetHttpOperations());

            // Check all built-in environments are loaded because discover is failed
            Assert.Equal(3, armEnvironments.Count);
            foreach (var env in armEnvironments.Values)
            {
                Assert.Equal(AzureEnvironment.TypeBuiltIn, env.Type);
            }
        }

        [Fact]
        public void TestDisableArmCloudMetadataInit()
        {
            Environment.SetEnvironmentVariable(ArmMetadataEnvVariable, "disabled");
            var armEnvironments = AzureEnvironment.InitializeBuiltInEnvironments(null, httpOperations: TestOperationsFactory.Create().GetHttpOperations());

            // Check all built-in environments are loaded because discover is disabled
            Assert.Equal(3, armEnvironments.Count);
            foreach (var env in armEnvironments.Values)
            {
                Assert.Equal(AzureEnvironment.TypeBuiltIn, env.Type);
            }
        }

        [Fact]
        public void TestArmResponseWithEmptyGalleryEndpoint()
        {
            Environment.SetEnvironmentVariable(ArmMetadataEnvVariable, @"TestData/ArmResponseWithEmptyGallery.json");
            var armEnvironments = AzureEnvironment.InitializeBuiltInEnvironments(null, httpOperations: TestOperationsFactory.Create().GetHttpOperations());

            Assert.Equal(3, armEnvironments.Count);
            Assert.Equal(AzureEnvironment.TypeDiscovered, armEnvironments[EnvironmentName.AzureCloud].Type);
            Assert.Equal(AzureEnvironment.TypeDiscovered, armEnvironments[EnvironmentName.AzureChinaCloud].Type);
            Assert.Equal(AzureEnvironment.TypeDiscovered, armEnvironments[EnvironmentName.AzureUSGovernment].Type);
            Assert.Null(armEnvironments[EnvironmentName.AzureCloud].GalleryUrl);
            Assert.Empty(armEnvironments[EnvironmentName.AzureChinaCloud].GalleryUrl);
            Assert.Empty(armEnvironments[EnvironmentName.AzureUSGovernment].GalleryUrl);
        }
    }
}
