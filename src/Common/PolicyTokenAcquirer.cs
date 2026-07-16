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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Commands.Common;
using Microsoft.Azure.Commands.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Pipeline-agnostic core that acquires an Azure Policy token for the change safety feature
    /// and stamps it onto an outgoing write request. This type owns the wire behavior (payload
    /// build, POST to the acquirePolicyToken endpoint, response handling and exception mapping)
    /// so it can be reused by different HTTP pipelines.
    /// </summary>
    public class PolicyTokenAcquirer
    {
        private const string TokenApiVersion = "2025-03-01";
        private const string PolicyTokenHeaderName = "x-ms-policy-external-evaluations";
        private const string LogPrefix = "[AcquirePolicyTokenHandler]";

        private static readonly Regex SubscriptionIdRegex = new Regex(@"/subscriptions/([0-9a-fA-F-]{36})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly HashSet<string> _allowedWriteMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethod.Put.Method,
            HttpMethod.Post.Method,
            HttpMethod.Delete.Method,
            "PATCH"
        };

        /// <summary>
        /// Acquires a policy token (when required) and attaches it to the outgoing request as the
        /// <c>x-ms-policy-external-evaluations</c> header. This performs everything the change safety
        /// pipeline does prior to forwarding the request to the inner handler.
        /// </summary>
        /// <param name="request">The outgoing request to inspect and (optionally) stamp.</param>
        /// <param name="shouldAcquire">Whether the user requested policy token acquisition.</param>
        /// <param name="changeReference">The change reference ID, or null if not specified.</param>
        /// <param name="isWhatIf">Whether -WhatIf was specified (dry run).</param>
        /// <param name="debugMessages">Queue for debug messages, or null.</param>
        /// <param name="tokenHttpClient">Optional HttpClient for the token API call (for testing).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task StampPolicyTokenAsync(
            HttpRequestMessage request,
            bool shouldAcquire,
            string changeReference,
            bool isWhatIf,
            ConcurrentQueue<string> debugMessages,
            HttpClient tokenHttpClient,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.RequestUri == null) throw new ArgumentException("RequestUri must be set.", nameof(request));

            EnqueueDebug(debugMessages, $"Intercept {request.Method} {request.RequestUri}");

            if (!_allowedWriteMethods.Contains(request.Method.Method))
            {
                EnqueueDebug(debugMessages, "Skip: verb not allowed for token acquisition.");
                return;
            }

            if (!shouldAcquire)
            {
                EnqueueDebug(debugMessages, "Skip: user did not request token (no -AcquirePolicyToken).");
                return;
            }

            if (isWhatIf)
            {
                EnqueueDebug(debugMessages, "Skip: -WhatIf present (dry run).");
                return;
            }

            try
            {
                var token = await AcquirePolicyTokenAsync(request, changeReference, debugMessages, tokenHttpClient, cancellationToken).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(token))
                {
                    if (request.Headers.Contains(PolicyTokenHeaderName))
                    {
                        request.Headers.Remove(PolicyTokenHeaderName);
                    }
                    request.Headers.Add(PolicyTokenHeaderName, token);
                    EnqueueDebug(debugMessages, "Token acquired and header added.");
                }
                else
                {
                    EnqueueDebug(debugMessages, "No token returned (null/empty).");
                }
            }
            catch (AzPSInvalidOperationException)
            {
                throw;
            }
            catch (AzPSCloudException)
            {
                throw;
            }
            catch (Exception ex)
            {
                EnqueueDebug(debugMessages, $"Exception: {ex.GetType().Name}: {ex.Message}");
                throw new AzPSInvalidOperationException(
                    $"Failed to acquire policy token for {request.Method} {request.RequestUri}: {ex.Message}",
                    ErrorKind.ServiceError,
                    ex,
                    desensitizedMessage: "Failed to acquire policy token.");
            }
        }

        private async Task<string> AcquirePolicyTokenAsync(
            HttpRequestMessage originalRequest,
            string changeReference,
            ConcurrentQueue<string> debugMessages,
            HttpClient tokenHttpClient,
            CancellationToken cancellationToken)
        {
            var subscriptionId = ExtractSubscriptionId(originalRequest.RequestUri);
            if (string.IsNullOrEmpty(subscriptionId))
            {
                EnqueueDebug(debugMessages, $"Failed: subscription id not found in URI {originalRequest.RequestUri}.");
                throw new AzPSInvalidOperationException(
                    $"Unable to determine subscription ID for policy token acquisition from URI: {originalRequest.RequestUri}",
                    ErrorKind.UserError,
                    desensitizedMessage: "Unable to determine subscription ID for policy token acquisition.");
            }

            var authority = originalRequest.RequestUri.GetLeftPart(UriPartial.Authority);
            var relativePath = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/acquirePolicyToken?api-version={TokenApiVersion}";
            var tokenUri = new Uri(authority + relativePath);

            object contentObj = null;
            if (originalRequest.Content != null)
            {
                await originalRequest.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                var body = await originalRequest.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        contentObj = JsonConvert.DeserializeObject(body);
                    }
                    catch
                    {
                        contentObj = body; // leave as raw string if not JSON
                    }
                }
            }

            var payload = new
            {
                operation = new
                {
                    uri = originalRequest.RequestUri.ToString(),
                    httpMethod = originalRequest.Method.Method,
                    content = contentObj
                },
                changeReference = changeReference
            };
            EnqueueDebug(debugMessages, "Payload prepared.");

            var payloadJson = JsonConvert.SerializeObject(payload);
            using (var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUri)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            })
            {
                tokenRequest.Headers.Add("x-ms-force-sync", "true");

                // Forward auth headers if present (minimal parity with original request auth context)
                if (originalRequest.Headers.Authorization != null)
                {
                    tokenRequest.Headers.Authorization = originalRequest.Headers.Authorization;
                }
                if (originalRequest.Headers.TryGetValues("x-ms-authorization-auxiliary", out var auxValues))
                {
                    tokenRequest.Headers.TryAddWithoutValidation("x-ms-authorization-auxiliary", auxValues);
                }
                if (originalRequest.Headers.TryGetValues("x-ms-client-request-id", out var clientRequestIdValues))
                {
                    tokenRequest.Headers.TryAddWithoutValidation("x-ms-client-request-id", clientRequestIdValues);
                }

                var isOwnedClient = tokenHttpClient == null;
                var http = tokenHttpClient ?? new HttpClient();
                try
                {
                    EnqueueDebug(debugMessages, $"POST acquirePolicyToken {tokenUri}");
                    using (var response = await http.SendAsync(tokenRequest, cancellationToken).ConfigureAwait(false))
                    {
                        EnqueueDebug(debugMessages, $"Response {(int)response.StatusCode} {response.StatusCode}");
                        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            if (!string.IsNullOrWhiteSpace(responseContent))
                            {
                                var obj = JsonConvert.DeserializeObject<JObject>(responseContent);
                                var token = obj?["token"]?.ToString();
                                if (string.IsNullOrEmpty(token))
                                {
                                    EnqueueDebug(debugMessages, "Response OK but token missing.");
                                    throw new AzPSCloudException(
                                        $"Policy token acquisition succeeded but no token was returned. Response: {responseContent}",
                                        ErrorKind.ServiceError,
                                        desensitizedMessage: "Policy token acquisition succeeded but no token was returned.");
                                }
                                return token;
                            }
                            throw new AzPSCloudException(
                                "Policy token acquisition returned an empty response body.",
                                ErrorKind.ServiceError,
                                desensitizedMessage: "Policy token acquisition returned an empty response body.");
                        }
                        else if (response.StatusCode == HttpStatusCode.Accepted)
                        {
                            EnqueueDebug(debugMessages, "202 Accepted received (async not supported).");
                            throw new AzPSCloudException(
                                "Asynchronous policy token acquisition (202 Accepted) is not supported.",
                                ErrorKind.ServiceError,
                                desensitizedMessage: "Asynchronous policy token acquisition (202 Accepted) is not supported.");
                        }
                        else
                        {
                            EnqueueDebug(debugMessages, $"Non-success status {(int)response.StatusCode}; will throw.");
                            throw new AzPSCloudException(
                                $"Policy token acquisition failed with {(int)response.StatusCode} {response.StatusCode}: {responseContent}",
                                ErrorKind.ServiceError,
                                desensitizedMessage: $"Policy token acquisition failed with status {(int)response.StatusCode}.");
                        }
                    }
                }
                finally
                {
                    if (isOwnedClient) http.Dispose();
                }
            }
        }

        private static string ExtractSubscriptionId(Uri uri)
        {
            if (uri == null) return null;
            var match = SubscriptionIdRegex.Match(uri.AbsolutePath);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        private static void EnqueueDebug(ConcurrentQueue<string> debugMessages, string message)
        {
            try
            {
                debugMessages?.Enqueue($"{LogPrefix} {message}");
            }
            catch { }
        }
    }
}
