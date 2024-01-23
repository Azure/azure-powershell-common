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

using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Core;
using Microsoft.Azure.Commands.ResourceManager.Common;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;

using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Test
{
    public class AzureRMCmdletAuthenticationUnitTests
    {

        private static readonly AzureAccount azureAccount = new AzureAccount()
        {
            Id = "fakeUserId",
            Type = AzureAccount.AccountType.User
        };

        private static IAzureSession CreateAuthenticationFactory()
        {
            var session = new Mock<IAzureSession>();
            var authFactory = new Mock<IAuthenticationFactory>();
            authFactory.Setup(f => f.Authenticate(
                It.IsAny<IAzureAccount>(),
                It.IsAny<IAzureEnvironment>(),
                It.IsAny<string>(),
                It.IsAny<SecureString>(),
                It.IsAny<string>(),
                It.IsAny<Action<string>>(),
                It.IsAny<string>())).Returns<IAzureAccount, IAzureEnvironment, string, SecureString, string, Action<string>, string>
                ((a, e, t, w, b, p, r) => new MockAccessToken(azureAccount.Id, t, azureAccount.Type, t));
            session.SetupProperty(m => m.AuthenticationFactory, authFactory.Object);
            return session.Object;
        }

        private static AzureRmProfileProvider CreateAzureRmProfile()
        {
            var context = new Mock<IAzureContext>();
            context.SetupProperty(m => m.Account, azureAccount);

            var profile = new Mock<IAzureContextContainer>();
            profile.SetupProperty(m => m.DefaultContext, context.Object);

            var profileProvider = new Mock<AzureRmProfileProvider>();
            profileProvider.SetupProperty(m => m.Profile, profile.Object);
            return profileProvider.Object;
        }

        private static void InitializeSessionAndProfile()
        {
            AzureSession.Initialize(CreateAuthenticationFactory, true);
            AzureSession.Modify(s => s.ARMContextSaveMode = ContextSaveMode.Process);
            AzureRmProfileProvider.SetInstance(() => CreateAzureRmProfile(), true);
        }

        private static void Dispose()
        {
            AzureRmProfileProvider.SetInstance(() => null, true);
            AzureSession.Modify(s => s = null);
        }

        public class MockCmdlet : AzureRMCmdlet
        {
            public IDictionary<string, IList<string>> GetAuxilaryAuthHeaderByTenatIds(IEnumerable<string> tenantIds)
            {
                return base.GetAuxiliaryAuthHeaderFromTenantIds(tenantIds);
            }

        }

        [Fact]
        public void TestGetAuxHeaderByTenantIds()
        {
            try
            {
                InitializeSessionAndProfile();
                var cmdlet = new MockCmdlet();
                var tenants = new List<string>() { "tenant0", "tenant1", "tenant2" };

                var header = cmdlet.GetAuxilaryAuthHeaderByTenatIds(tenants);

                Assert.Equal(1, header.Count);
                var h = header.First();
                Assert.Equal("x-ms-authorization-auxiliary", h.Key);
                Assert.Single(h.Value);
                var tokens = h.Value.First().Split(';');
                var regex = new Regex(@"Bearer ([0-9a-zA-Z]+)");
                for (int i = 0; i < tenants.Count; ++i)
                {
                    var match = regex.Match(tokens[i]);
                    Assert.True(match.Success);
                    match.Groups[1].Value.StartsWith(tenants[i]);
                }
            }
            finally
            {
                Dispose();
            }
        }

        [Fact]
        public void TestGetAuxHeaderByEmptyTenantIds()
        {
            try
            {
                InitializeSessionAndProfile();
                var cmdlet = new MockCmdlet();
                var tenants = new List<string>();

                var header = cmdlet.GetAuxilaryAuthHeaderByTenatIds(tenants);

                Assert.Null(header);
            }
            finally
            {
                Dispose();
            }
        }

        [Fact]
        public void TestGetAuxHeaderByExcessTenantIds()
        {
            try
            {
                InitializeSessionAndProfile();
                var cmdlet = new MockCmdlet();
                var tenants = new List<string>() { "tenant0", "tenant1", "tenant2", "tenants3" };

                Assert.Throws<ArgumentException>(() => cmdlet.GetAuxilaryAuthHeaderByTenatIds(tenants));
            }
            finally
            {
                Dispose();
            }
        }
    }
}
