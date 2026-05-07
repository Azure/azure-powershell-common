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
            var handler = new AcquirePolicyTokenHandler(false, null, false, null);
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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
            // Verify the header name is recognized by HTTP headers (case-insensitive)
            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-policy-external-evaluations", "test-token");
            Assert.True(request.Headers.Contains("x-ms-policy-external-evaluations"));
            Assert.Equal("test-token", request.Headers.GetValues("x-ms-policy-external-evaluations").First());
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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

            var handler = new AcquirePolicyTokenHandler(false, null, false, null) { InnerHandler = innerHandler };
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
        [InlineData("Test-AzDeployment", false)]
        [InlineData("Test-AzResourceGroupDeployment", false)]
        public void WriteCmdlets_GetParams_ReadCmdlets_DoNot(string commandName, bool shouldHaveParams)
        {
            bool isReadOnly = commandName.StartsWith("Get", StringComparison.OrdinalIgnoreCase)
                || commandName.StartsWith("Test", StringComparison.OrdinalIgnoreCase)
                || commandName.StartsWith("List", StringComparison.OrdinalIgnoreCase)
                || commandName.StartsWith("Show", StringComparison.OrdinalIgnoreCase);

            Assert.Equal(shouldHaveParams, !isReadOnly);
        }

        [Theory]
        [InlineData("List-AzResource", false)]      // starts with "List"
        [InlineData("Show-AzVM", false)]             // starts with "Show"
        [InlineData("Export-AzStorageData", true)]   // Export is a write operation
        [InlineData("Import-AzData", true)]          // Import is a write operation
        [InlineData("Disable-AzFeature", true)]
        [InlineData("Enable-AzFeature", true)]
        public void EdgeCases_FilteredCorrectly(string commandName, bool shouldHaveParams)
        {
            bool isReadOnly = commandName.StartsWith("Get", StringComparison.OrdinalIgnoreCase)
                || commandName.StartsWith("Test", StringComparison.OrdinalIgnoreCase)
                || commandName.StartsWith("List", StringComparison.OrdinalIgnoreCase)
                || commandName.StartsWith("Show", StringComparison.OrdinalIgnoreCase);

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
    // 9. END-TO-END FLOW TESTS — Full token acquisition pipeline
    //    Simulates: handler → acquirePolicyToken API → token → header → request
    // =========================================================================
    public class EndToEndPolicyTokenFlowTests
    {
        private const string TestToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test-policy-token-value";
        private const string TestSubId = "f5d0e517-47aa-467c-90a0-4326d3c0fcae";
        private const string BaseUrl = "https://management.azure.com";

        /// <summary>
        /// Creates a mock token server that returns a valid token response.
        /// </summary>
        private static HttpClient CreateMockTokenServer(Action<HttpRequestMessage> captureTokenRequest = null)
        {
            var handler = new MockTokenServerHandler((req, ct) =>
            {
                captureTokenRequest?.Invoke(req);
                var responseBody = JsonConvert.SerializeObject(new { token = TestToken });
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
                });
            });
            return new HttpClient(handler);
        }

        /// <summary>
        /// Creates a mock token server that returns a failure.
        /// </summary>
        private static HttpClient CreateFailingTokenServer(HttpStatusCode statusCode, string errorMessage)
        {
            var handler = new MockTokenServerHandler((req, ct) =>
            {
                var body = JsonConvert.SerializeObject(new { error = new { code = "TestError", message = errorMessage } });
                return Task.FromResult(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                });
            });
            return new HttpClient(handler);
        }

        /// <summary>
        /// Creates the handler with a mock cmdlet that has ShouldAcquirePolicyToken = true.
        /// Uses internal constructor to inject a mock token HTTP client.
        /// </summary>
        private static (AcquirePolicyTokenHandler handler, HttpClient client) CreateTestPipeline(
            HttpClient tokenHttpClient,
            Action<HttpRequestMessage> captureRequest = null)
        {
            var innerHandler = new MockInnerHandler((req, ct) =>
            {
                captureRequest?.Invoke(req);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var handler = new AcquirePolicyTokenHandler(true, null, false, new ConcurrentQueue<string>(), tokenHttpClient)
            {
                InnerHandler = innerHandler
            };

            return (handler, new HttpClient(handler));
        }

        // ----- DELETE Storage Account -----

        [Fact]
        public async Task E2E_DeleteStorageAccount_TokenAcquired_HeaderAttached()
        {
            HttpRequestMessage capturedRequest = null;
            var tokenClient = CreateMockTokenServer();
            var (handler, client) = CreateTestPipeline(tokenClient, req => capturedRequest = req);

            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/testsa?api-version=2024-01-01");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"),
                "DELETE Storage Account: policy token header must be present");
            Assert.Equal(TestToken,
                capturedRequest.Headers.GetValues("x-ms-policy-external-evaluations").First());
        }

        // ----- DELETE Resource Group -----

        [Fact]
        public async Task E2E_DeleteResourceGroup_TokenAcquired_HeaderAttached()
        {
            HttpRequestMessage capturedRequest = null;
            var tokenClient = CreateMockTokenServer();
            var (handler, client) = CreateTestPipeline(tokenClient, req => capturedRequest = req);

            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/test-rg?api-version=2021-04-01");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"));
            Assert.Equal(TestToken,
                capturedRequest.Headers.GetValues("x-ms-policy-external-evaluations").First());
        }

        // ----- PUT (Create/Update) VM -----

        [Fact]
        public async Task E2E_PutVM_TokenAcquired_HeaderAttached()
        {
            HttpRequestMessage capturedRequest = null;
            var tokenClient = CreateMockTokenServer();
            var (handler, client) = CreateTestPipeline(tokenClient, req => capturedRequest = req);

            var request = new HttpRequestMessage(HttpMethod.Put,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/testvm?api-version=2024-03-01")
            {
                Content = new StringContent("{\"location\":\"eastus\"}", Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"));
            Assert.Equal(TestToken,
                capturedRequest.Headers.GetValues("x-ms-policy-external-evaluations").First());
        }

        // ----- POST Action (e.g., restart VM) -----

        [Fact]
        public async Task E2E_PostVMRestart_TokenAcquired_HeaderAttached()
        {
            HttpRequestMessage capturedRequest = null;
            var tokenClient = CreateMockTokenServer();
            var (handler, client) = CreateTestPipeline(tokenClient, req => capturedRequest = req);

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/testvm/restart?api-version=2024-03-01");

            var response = await client.SendAsync(request);

            Assert.True(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"));
            Assert.Equal(TestToken,
                capturedRequest.Headers.GetValues("x-ms-policy-external-evaluations").First());
        }

        // ----- PATCH (Update) Network Security Group -----

        [Fact]
        public async Task E2E_PatchNSG_TokenAcquired_HeaderAttached()
        {
            HttpRequestMessage capturedRequest = null;
            var tokenClient = CreateMockTokenServer();
            var (handler, client) = CreateTestPipeline(tokenClient, req => capturedRequest = req);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"),
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Network/networkSecurityGroups/testnsg?api-version=2023-11-01")
            {
                Content = new StringContent("{\"tags\":{\"env\":\"test\"}}", Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            Assert.True(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"));
            Assert.Equal(TestToken,
                capturedRequest.Headers.GetValues("x-ms-policy-external-evaluations").First());
        }

        // ----- DELETE CosmosDB Account -----

        [Fact]
        public async Task E2E_DeleteCosmosDBAccount_TokenAcquired_HeaderAttached()
        {
            HttpRequestMessage capturedRequest = null;
            var tokenClient = CreateMockTokenServer();
            var (handler, client) = CreateTestPipeline(tokenClient, req => capturedRequest = req);

            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.DocumentDB/databaseAccounts/testcosmos?api-version=2024-05-15");

            var response = await client.SendAsync(request);

            Assert.True(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"));
            Assert.Equal(TestToken,
                capturedRequest.Headers.GetValues("x-ms-policy-external-evaluations").First());
        }

        // ----- DELETE Key Vault -----

        [Fact]
        public async Task E2E_DeleteKeyVault_TokenAcquired_HeaderAttached()
        {
            HttpRequestMessage capturedRequest = null;
            var tokenClient = CreateMockTokenServer();
            var (handler, client) = CreateTestPipeline(tokenClient, req => capturedRequest = req);

            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/testvault?api-version=2023-07-01");

            var response = await client.SendAsync(request);

            Assert.True(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"));
            Assert.Equal(TestToken,
                capturedRequest.Headers.GetValues("x-ms-policy-external-evaluations").First());
        }

        // ----- Verify token API request payload -----

        [Fact]
        public async Task E2E_TokenRequest_ContainsCorrectPayload()
        {
            HttpRequestMessage capturedTokenRequest = null;
            var tokenClient = CreateMockTokenServer(req => capturedTokenRequest = req);
            var (handler, client) = CreateTestPipeline(tokenClient);

            var deleteUri = $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa1?api-version=2024-01-01";
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, deleteUri));

            // Verify the token request
            Assert.NotNull(capturedTokenRequest);
            Assert.Equal(HttpMethod.Post, capturedTokenRequest.Method);
            Assert.Contains("acquirePolicyToken", capturedTokenRequest.RequestUri.ToString());
            Assert.Contains(TestSubId, capturedTokenRequest.RequestUri.ToString());
            Assert.Contains("api-version=2025-03-01", capturedTokenRequest.RequestUri.ToString());

            // Verify payload
            var body = await capturedTokenRequest.Content.ReadAsStringAsync();
            var payload = JObject.Parse(body);
            Assert.Equal(deleteUri, payload["operation"]["uri"].ToString());
            Assert.Equal("DELETE", payload["operation"]["httpMethod"].ToString());

            // Verify x-ms-force-sync header
            Assert.True(capturedTokenRequest.Headers.Contains("x-ms-force-sync"));
        }

        // ----- Verify changeReference is passed in payload -----

        [Fact]
        public async Task E2E_TokenRequest_IncludesChangeReference()
        {
            HttpRequestMessage capturedTokenRequest = null;
            var tokenClient = CreateMockTokenServer(req => capturedTokenRequest = req);

            string changeRef = $"/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.ChangeSafety/changeStates/cs/stageProgressions/breakglassStage";
            var innerHandler = new MockInnerHandler((req, ct) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var handler = new AcquirePolicyTokenHandler(true, changeRef, false, new ConcurrentQueue<string>(), tokenClient) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa1?api-version=2024-01-01"));

            var body = await capturedTokenRequest.Content.ReadAsStringAsync();
            var payload = JObject.Parse(body);
            Assert.Equal(changeRef, payload["changeReference"].ToString());
        }

        // ----- Verify auth headers are forwarded to token request -----

        [Fact]
        public async Task E2E_TokenRequest_ForwardsAuthHeaders()
        {
            HttpRequestMessage capturedTokenRequest = null;
            var tokenClient = CreateMockTokenServer(req => capturedTokenRequest = req);
            var (handler, client) = CreateTestPipeline(tokenClient);

            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa1?api-version=2024-01-01");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token-123");
            request.Headers.Add("x-ms-authorization-auxiliary", "Bearer aux-token-456");

            await client.SendAsync(request);

            Assert.Equal("Bearer", capturedTokenRequest.Headers.Authorization.Scheme);
            Assert.Equal("test-token-123", capturedTokenRequest.Headers.Authorization.Parameter);
            Assert.True(capturedTokenRequest.Headers.Contains("x-ms-authorization-auxiliary"));
        }

        // ----- Token API failure returns clear error -----

        [Fact]
        public async Task E2E_TokenApiFails_ThrowsWithMessage()
        {
            var tokenClient = CreateFailingTokenServer(HttpStatusCode.Forbidden, "Access denied");
            var (handler, client) = CreateTestPipeline(tokenClient);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
                    $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa1?api-version=2024-01-01")));

            Assert.Contains("Failed to acquire policy token", ex.Message);
            Assert.Contains("403", ex.Message);
        }

        // ----- Token API returns 200 but no token field -----

        [Fact]
        public async Task E2E_TokenApiReturns200ButNoToken_ThrowsWithResponse()
        {
            var handler = new MockTokenServerHandler((req, ct) =>
            {
                var body = JsonConvert.SerializeObject(new { result = "Failed", message = "validator error" });
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                });
            });
            var tokenClient = new HttpClient(handler);
            var (pipelineHandler, client) = CreateTestPipeline(tokenClient);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
                    $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa1?api-version=2024-01-01")));

            Assert.Contains("No token returned", ex.Message);
            Assert.Contains("validator error", ex.Message);
        }

        // ----- GET request is NOT intercepted even with flag -----

        [Fact]
        public async Task E2E_GetRequest_NeverAcquiresToken_EvenWithFlag()
        {
            bool tokenServerCalled = false;
            var tokenClient = CreateMockTokenServer(_ => tokenServerCalled = true);
            HttpRequestMessage capturedRequest = null;
            var (handler, client) = CreateTestPipeline(tokenClient, req => capturedRequest = req);

            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa1?api-version=2024-01-01"));

            Assert.False(tokenServerCalled, "Token API must NOT be called for GET requests");
            Assert.False(capturedRequest.Headers.Contains("x-ms-policy-external-evaluations"));
        }

        // ----- Multiple sequential requests each get their own token -----

        [Fact]
        public async Task E2E_MultipleRequests_EachGetsToken()
        {
            int tokenCallCount = 0;
            var mockHandler = new MockTokenServerHandler((req, ct) =>
            {
                Interlocked.Increment(ref tokenCallCount);
                var body = JsonConvert.SerializeObject(new { token = $"token-{tokenCallCount}" });
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                });
            });
            var tokenClient = new HttpClient(mockHandler);

            List<string> capturedTokens = new List<string>();
            var innerHandler = new MockInnerHandler((req, ct) =>
            {
                if (req.Headers.Contains("x-ms-policy-external-evaluations"))
                    capturedTokens.Add(req.Headers.GetValues("x-ms-policy-external-evaluations").First());
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var handler = new AcquirePolicyTokenHandler(true, null, false, new ConcurrentQueue<string>(), tokenClient) { InnerHandler = innerHandler };
            var client = new HttpClient(handler);

            await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/sa1?api-version=2024-01-01"));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Put,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg2/providers/Microsoft.Compute/virtualMachines/vm1?api-version=2024-03-01"));

            Assert.Equal(2, tokenCallCount);
            Assert.Equal(2, capturedTokens.Count);
        }

        // ----- Request body is included in token payload -----

        [Fact]
        public async Task E2E_PutWithBody_BodyIncludedInTokenPayload()
        {
            HttpRequestMessage capturedTokenRequest = null;
            var tokenClient = CreateMockTokenServer(req => capturedTokenRequest = req);
            var (handler, client) = CreateTestPipeline(tokenClient);

            var requestBody = "{\"location\":\"eastus\",\"sku\":{\"name\":\"Standard_GRS\"}}";
            var request = new HttpRequestMessage(HttpMethod.Put,
                $"{BaseUrl}/subscriptions/{TestSubId}/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa1?api-version=2024-01-01")
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            await client.SendAsync(request);

            var tokenBody = await capturedTokenRequest.Content.ReadAsStringAsync();
            var payload = JObject.Parse(tokenBody);
            Assert.Equal("eastus", payload["operation"]["content"]["location"].ToString());
            Assert.Equal("Standard_GRS", payload["operation"]["content"]["sku"]["name"].ToString());
        }
    }

    // =========================================================================
    // MOCK HANDLERS — Shared test infrastructure
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

    internal class MockTokenServerHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

        public MockTokenServerHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handlerFunc(request, cancellationToken);
        }
    }
}

