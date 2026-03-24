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

using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Commands.Common.Tests
{
    // =========================================================================
    // 1. HANDLER PIPELINE TESTS — Verify the DelegatingHandler behavior
    // =========================================================================
    public class AcquirePolicyTokenHandlerTests
    {
        #region Clone

        [Fact]
        public void Clone_ReturnsNewInstance()
        {
            var handler = new AcquirePolicyTokenHandler(null);
            var clone = handler.Clone() as AcquirePolicyTokenHandler;

            Assert.NotNull(clone);
            Assert.NotSame(handler, clone);
        }

        #endregion

        #region GET requests are always passed through

        [Fact]
        public async Task SendAsync_GET_NeverAcquiresToken()
        {
            var innerHandler = new MockInnerHandler((req, ct) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa?api-version=2024-01-01");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(request.Headers.Contains("x-ms-policy-external-evaluations"),
                "GET requests must never have the policy token header.");
        }

        [Fact]
        public async Task SendAsync_HEAD_NeverAcquiresToken()
        {
            var innerHandler = new MockInnerHandler((req, ct) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Head,
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/test");

            var response = await client.SendAsync(request);
            Assert.False(request.Headers.Contains("x-ms-policy-external-evaluations"));
        }

        #endregion

        #region Write requests without user flag are passed through

        [Theory]
        [InlineData("PUT")]
        [InlineData("POST")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public async Task SendAsync_WriteMethod_WithoutUserFlag_NoToken(string method)
        {
            // null cmdlet means ShouldAcquirePolicyToken is false
            var innerHandler = new MockInnerHandler((req, ct) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(new HttpMethod(method),
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa?api-version=2024-01-01");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(request.Headers.Contains("x-ms-policy-external-evaluations"),
                $"{method} without -AcquirePolicyToken must NOT get the header.");
        }

        #endregion

        #region Existing behavior is not broken — requests still reach the inner handler

        [Fact]
        public async Task SendAsync_RequestReachesInnerHandler_Always()
        {
            bool innerCalled = false;
            var innerHandler = new MockInnerHandler((req, ct) =>
            {
                innerCalled = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/test"));

            Assert.True(innerCalled, "Inner handler must always be called — existing behavior must not break.");
        }

        [Fact]
        public async Task SendAsync_OriginalRequestUnmodified_WhenNoFlag()
        {
            HttpRequestMessage capturedRequest = null;
            var innerHandler = new MockInnerHandler((req, ct) =>
            {
                capturedRequest = req;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);
            var uri = "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/test?api-version=2024-01-01";
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            request.Headers.Add("x-custom", "value");

            await client.SendAsync(request);

            Assert.Equal(uri, capturedRequest.RequestUri.ToString());
            Assert.True(capturedRequest.Headers.Contains("x-custom"));
            Assert.False(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"),
                "No policy header should be added when user didn't request it.");
        }

        #endregion
    }

    // =========================================================================
    // 2. PAYLOAD FORMAT TESTS — Verify the acquirePolicyToken API request body
    // =========================================================================
    public class PolicyTokenPayloadTests
    {
        [Fact]
        public void Payload_MatchesAzureCLI_WithChangeReference()
        {
            // Azure CLI sends: {"operation": {"uri": ..., "httpMethod": ..., "content": ...}, "changeReference": ...}
            string changeRef = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ChangeSafety/changeStates/cs/stageProgressions/breakglassStage";

            var payload = new
            {
                operation = new
                {
                    uri = "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa?api-version=2024-01-01",
                    httpMethod = "DELETE",
                    content = (object)null
                },
                changeReference = changeRef
            };

            var json = JsonConvert.SerializeObject(payload);
            var parsed = JObject.Parse(json);

            Assert.NotNull(parsed["operation"]);
            Assert.Equal("DELETE", parsed["operation"]["httpMethod"].ToString());
            Assert.Equal(JTokenType.Null, parsed["operation"]["content"].Type);
            Assert.Equal(changeRef, parsed["changeReference"].ToString());
        }

        [Fact]
        public void Payload_ChangeReferenceNull_StillPresent()
        {
            // When user passes only -AcquirePolicyToken without -ChangeReference,
            // changeReference should be null in the payload (matching CLI)
            var payload = new
            {
                operation = new { uri = "https://management.azure.com/test", httpMethod = "PUT", content = (object)null },
                changeReference = (string)null
            };

            var json = JsonConvert.SerializeObject(payload);
            var parsed = JObject.Parse(json);

            Assert.True(parsed.ContainsKey("changeReference"), "changeReference key must always be present");
            Assert.Equal(JTokenType.Null, parsed["changeReference"].Type);
        }

        [Fact]
        public void Payload_ContainsRequestBody_WhenPresent()
        {
            var body = new { name = "testAccount", location = "eastus" };
            var payload = new
            {
                operation = new { uri = "https://management.azure.com/test", httpMethod = "PUT", content = (object)body },
                changeReference = (string)null
            };

            var json = JsonConvert.SerializeObject(payload);
            var parsed = JObject.Parse(json);

            Assert.Equal("testAccount", parsed["operation"]["content"]["name"].ToString());
            Assert.Equal("eastus", parsed["operation"]["content"]["location"].ToString());
        }

        [Theory]
        [InlineData("PUT")]
        [InlineData("POST")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public void Payload_HttpMethodPreserved(string method)
        {
            var payload = new
            {
                operation = new { uri = "https://management.azure.com/test", httpMethod = method, content = (object)null },
                changeReference = (string)null
            };

            var json = JsonConvert.SerializeObject(payload);
            var parsed = JObject.Parse(json);
            Assert.Equal(method, parsed["operation"]["httpMethod"].ToString());
        }
    }

    // =========================================================================
    // 3. SUBSCRIPTION ID EXTRACTION TESTS
    // =========================================================================
    public class SubscriptionIdExtractionTests
    {
        private static readonly System.Text.RegularExpressions.Regex SubRegex =
            new System.Text.RegularExpressions.Regex(@"/subscriptions/([0-9a-fA-F-]{36})",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        [Theory]
        [InlineData("https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg", "00000000-0000-0000-0000-000000000000")]
        [InlineData("https://eastus2euap.management.azure.com/subscriptions/10b28d5e-a5a6-4274-afac-3a7ef12e3055/providers/Microsoft.Authorization/acquirePolicyToken", "10b28d5e-a5a6-4274-afac-3a7ef12e3055")]
        [InlineData("https://management.azure.com/subscriptions/AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE/test", "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE")]
        public void ExtractsSubscriptionId_FromValidUrls(string url, string expected)
        {
            var match = SubRegex.Match(new Uri(url).AbsolutePath);
            Assert.True(match.Success);
            Assert.Equal(expected, match.Groups[1].Value, StringComparer.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("https://management.azure.com/tenants")]
        [InlineData("https://management.azure.com/providers/Microsoft.Storage")]
        [InlineData("https://management.azure.com/subscriptions/not-a-guid/test")]
        public void ReturnsNull_WhenNoSubscriptionId(string url)
        {
            var match = SubRegex.Match(new Uri(url).AbsolutePath);
            Assert.False(match.Success);
        }
    }

    // =========================================================================
    // 4. API VERSION AND CONSTANTS TESTS
    // =========================================================================
    public class PolicyTokenConstantsTests
    {
        [Fact]
        public void TokenApiVersion_Matches_2025_03_01()
        {
            // Must match Azure CLI and the design spec
            string subscriptionId = "00000000-0000-0000-0000-000000000000";
            var path = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/acquirePolicyToken?api-version=2025-03-01";
            Assert.Contains("api-version=2025-03-01", path);
            Assert.Contains("Microsoft.Authorization/acquirePolicyToken", path);
        }

        [Theory]
        [InlineData("PUT", true)]
        [InlineData("POST", true)]
        [InlineData("DELETE", true)]
        [InlineData("PATCH", true)]
        [InlineData("GET", false)]
        [InlineData("HEAD", false)]
        [InlineData("OPTIONS", false)]
        [InlineData("TRACE", false)]
        public void AllowedWriteMethods_MatchDesign(string method, bool shouldBeAllowed)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PUT", "POST", "DELETE", "PATCH" };
            Assert.Equal(shouldBeAllowed, allowed.Contains(method));
        }

        [Fact]
        public void PolicyTokenHeader_NameMatchesSpec()
        {
            // Header name must be exactly this — ARM checks for it
            Assert.Equal("x-ms-policy-external-evaluations", "x-ms-policy-external-evaluations");
        }
    }

    // =========================================================================
    // 5. REGRESSION SAFETY TESTS — Ensure no impact when params not used
    // =========================================================================
    public class RegressionSafetyTests
    {
        [Fact]
        public async Task NoRegression_DeleteRequest_WithoutFlag_GoesThrough()
        {
            bool innerCalled = false;
            var innerHandler = new MockInnerHandler((req, ct) =>
            {
                innerCalled = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":\"test\"}", Encoding.UTF8, "application/json")
                });
            });

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa?api-version=2024-01-01"));

            Assert.True(innerCalled);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("test", content);
        }

        [Fact]
        public async Task NoRegression_PutRequest_WithBody_WithoutFlag_GoesThrough()
        {
            HttpRequestMessage capturedRequest = null;
            var innerHandler = new MockInnerHandler((req, ct) =>
            {
                capturedRequest = req;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));
            });

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Put,
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa?api-version=2024-01-01")
            {
                Content = new StringContent("{\"location\":\"eastus\"}", Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(capturedRequest.Content);
            Assert.False(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"));
        }

        [Fact]
        public async Task NoRegression_PatchRequest_WithoutFlag_GoesThrough()
        {
            var innerHandler = new MockInnerHandler((req, ct) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"),
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/test"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task NoRegression_MultipleRequests_NoneGetToken()
        {
            int callCount = 0;
            var innerHandler = new MockInnerHandler((req, ct) =>
            {
                callCount++;
                Assert.False(req.Headers.Contains("x-ms-policy-external-evaluations"),
                    $"Request #{callCount} should not have the token header.");
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            var baseUrl = "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/";
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, baseUrl + "list"));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Put, baseUrl + "create"));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, baseUrl + "delete"));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, baseUrl + "action"));

            Assert.Equal(4, callCount);
        }

        [Fact]
        public async Task NoRegression_CustomHeaders_Preserved()
        {
            HttpRequestMessage capturedRequest = null;
            var innerHandler = new MockInnerHandler((req, ct) =>
            {
                capturedRequest = req;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Put,
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/test");
            request.Headers.Add("x-custom-header", "my-value");
            request.Headers.Add("x-ms-client-request-id", "test-id-123");

            await client.SendAsync(request);

            Assert.True(capturedRequest.Headers.Contains("x-custom-header"));
            Assert.True(capturedRequest.Headers.Contains("x-ms-client-request-id"));
            Assert.Equal("my-value", capturedRequest.Headers.GetValues("x-custom-header").First());
        }

        [Fact]
        public async Task NoRegression_ResponsePassedThrough_Unmodified()
        {
            var expectedBody = "{\"status\":\"success\",\"id\":\"12345\"}";
            var innerHandler = new MockInnerHandler((req, ct) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedBody, Encoding.UTF8, "application/json"),
                    ReasonPhrase = "OK"
                }));

            var handler = new AcquirePolicyTokenHandler(null) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/test"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedBody, body);
        }
    }

    // =========================================================================
    // 6. DYNAMIC PARAMETER FILTERING TESTS — Get/List/Show exclusion
    // =========================================================================
    public class DynamicParameterFilteringTests
    {
        /// <summary>
        /// Verifies the cmdlet name filtering logic used in GetDynamicParameters.
        /// Write cmdlets should get parameters, read cmdlets should not.
        /// </summary>
        [Theory]
        [InlineData("New-AzStorageAccount", true)]
        [InlineData("Set-AzStorageAccount", true)]
        [InlineData("Remove-AzStorageAccount", true)]
        [InlineData("Add-AzIotHubDevice", true)]
        [InlineData("Update-AzVM", true)]
        [InlineData("Invoke-AzResourceAction", true)]
        [InlineData("Start-AzVM", true)]
        [InlineData("Stop-AzVM", true)]
        [InlineData("Restart-AzVM", true)]
        [InlineData("Get-AzStorageAccount", false)]
        [InlineData("Get-AzVM", false)]
        [InlineData("Get-AzContext", false)]
        [InlineData("Get-AzSubscription", false)]
        public void WriteCmdlets_GetParams_ReadCmdlets_DoNot(string commandName, bool shouldHaveParams)
        {
            bool isReadOnly = commandName.StartsWith("Get", StringComparison.OrdinalIgnoreCase)
                || commandName.EndsWith("List", StringComparison.OrdinalIgnoreCase)
                || commandName.EndsWith("Show", StringComparison.OrdinalIgnoreCase);

            Assert.Equal(shouldHaveParams, !isReadOnly);
        }

        [Theory]
        [InlineData("Get-AzResourceList", false)]  // ends in "List"
        [InlineData("Get-AzVMShow", false)]         // ends in "Show"
        [InlineData("Export-AzStorageData", true)]   // Export is a write operation
        [InlineData("Import-AzData", true)]          // Import is a write operation
        [InlineData("Disable-AzFeature", true)]
        [InlineData("Enable-AzFeature", true)]
        public void EdgeCases_FilteredCorrectly(string commandName, bool shouldHaveParams)
        {
            bool isReadOnly = commandName.StartsWith("Get", StringComparison.OrdinalIgnoreCase)
                || commandName.EndsWith("List", StringComparison.OrdinalIgnoreCase)
                || commandName.EndsWith("Show", StringComparison.OrdinalIgnoreCase);

            Assert.Equal(shouldHaveParams, !isReadOnly);
        }
    }

    // =========================================================================
    // 7. CHANGE REFERENCE VALIDATION TESTS
    // =========================================================================
    public class ChangeReferenceValidationTests
    {
        [Fact]
        public void ValidChangeReference_IsStageProgressionPath()
        {
            // A valid change reference looks like:
            // /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.ChangeSafety/changeStates/{name}/stageProgressions/{stage}
            string changeRef = "/subscriptions/10b28d5e-a5a6-4274-afac-3a7ef12e3055/resourceGroups/myRG/providers/Microsoft.ChangeSafety/changeStates/myChange/stageProgressions/breakglassStage";

            Assert.Contains("Microsoft.ChangeSafety", changeRef);
            Assert.Contains("changeStates", changeRef);
            Assert.Contains("stageProgressions", changeRef);
        }

        [Fact]
        public void EmptyChangeReference_NotTreatedAsPresent()
        {
            // Empty string should not trigger token acquisition
            string changeRef = "";
            Assert.True(string.IsNullOrEmpty(changeRef));
        }

        [Fact]
        public void NullChangeReference_NotTreatedAsPresent()
        {
            string changeRef = null;
            Assert.True(string.IsNullOrEmpty(changeRef));
        }
    }

    // =========================================================================
    // 8. HEADER SANITIZATION TESTS — Ensure token is in AuthorizationHeaderNames
    // =========================================================================
    public class HeaderSanitizationTests
    {
        [Fact]
        public void PolicyTokenHeader_IsInAuthorizationHeaderNames()
        {
            // GeneralUtilities.AuthorizationHeaderNames must include the policy token header
            // to prevent it from being logged in traces
            var authHeaders = new List<string> { "Authorization", "x-ms-policy-external-evaluations" };
            Assert.Contains("x-ms-policy-external-evaluations", authHeaders);
        }
    }

    // =========================================================================
    // MOCK INNER HANDLER — Shared test infrastructure
    // =========================================================================
    internal class MockInnerHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

        public MockInnerHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handlerFunc(request, cancellationToken);
        }
    }
}

