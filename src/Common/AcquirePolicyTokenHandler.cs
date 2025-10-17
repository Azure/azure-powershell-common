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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Delegating handler to acquire an Azure Policy token for change safety feature and attach to outgoing request.
    /// Activated when user specifies -AcquirePolicyToken. (ChangeReference deferred to Phase 2.)
    /// </summary>
    public class AcquirePolicyTokenHandler : DelegatingHandler, ICloneable
    {
        private readonly AzurePSCmdlet _cmdlet;
        private const string TokenApiVersion = "2025-03-01";
        private static readonly Regex SubscriptionIdRegex = new Regex(@"/subscriptions/([0-9a-fA-F-]{36})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly HashSet<string> _allowedWriteMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethod.Put.Method,
            HttpMethod.Post.Method,
            HttpMethod.Delete.Method,
            "PATCH"
        };
        private const string LogPrefix = "[AcquirePolicyTokenHandler]";

        public AcquirePolicyTokenHandler(AzurePSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            EnqueueDebug($"Intercept {request.Method} {request.RequestUri}");

            if (!(_cmdlet?.IsPolicyTokenFeatureEnabled() ?? false))
            {
                EnqueueDebug("Skip: feature disabled (EnableAcquirePolicyToken config set to false).");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            bool allowedVerb = _allowedWriteMethods.Contains(request.Method.Method);
            if (!allowedVerb)
            {
                EnqueueDebug("Skip: verb not allowed for token acquisition.");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            bool hasCmdlet = _cmdlet != null;
            bool userRequested = hasCmdlet && _cmdlet.ShouldAcquirePolicyToken;
            if (!userRequested)
            {
                EnqueueDebug("Skip: user did not request token (no -AcquirePolicyToken).");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            var isWhatIf = _cmdlet.MyInvocation?.BoundParameters?.ContainsKey("WhatIf") == true;
            if (isWhatIf)
            {
                EnqueueDebug("Skip: -WhatIf present (dry run).");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            try
            {
                var token = await AcquirePolicyTokenAsync(request, cancellationToken).ConfigureAwait(false);
                
                //Debug token, as is
                // EnqueueDebug($"Token: {token}");
                
                
                if (!string.IsNullOrEmpty(token))
                {
                    if (request.Headers.Contains("x-ms-policy-external-evaluations"))
                    {
                        request.Headers.Remove("x-ms-policy-external-evaluations");
                    }
                    request.Headers.Add("x-ms-policy-external-evaluations", token);
                    EnqueueDebug("Token acquired and header added.");
                }
                else
                {
                    EnqueueDebug("No token returned (null/empty).");
                }
            }
            catch (Exception ex)
            {
                EnqueueDebug($"Exception: {ex.GetType().Name}: {ex.Message}");
                throw new InvalidOperationException($"Failed to acquire policy token: {ex.Message}", ex);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> AcquirePolicyTokenAsync(HttpRequestMessage originalRequest, CancellationToken cancellationToken)
        {
            var subscriptionId = ExtractSubscriptionId(originalRequest.RequestUri);
            if (string.IsNullOrEmpty(subscriptionId))
            {
                EnqueueDebug("Failed: subscription id not found in URI.");
                throw new InvalidOperationException("Unable to determine subscription ID for policy token acquisition.");
            }

            var authority = originalRequest.RequestUri.GetLeftPart(UriPartial.Authority);
            var relativePath = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/acquirePolicyToken?api-version={TokenApiVersion}";
            var tokenUri = new Uri(authority + relativePath);

            object contentObj = null;
            if (originalRequest.Content != null)
            {
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
                }
                // Phase 2: reintroduce when ChangeReference parameter is enabled
                // ,changeReference = _cmdlet?.CurrentChangeReference
            };
            EnqueueDebug("Payload prepared.");

            var payloadJson = JsonConvert.SerializeObject(payload);
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUri)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            };
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

            using (var http = new HttpClient())
            {
                EnqueueDebug($"POST acquirePolicyToken {tokenUri}");
                var response = await http.SendAsync(tokenRequest, cancellationToken).ConfigureAwait(false);
                EnqueueDebug($"Response {(int)response.StatusCode} {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        var obj = JsonConvert.DeserializeObject<JObject>(responseContent);
                        var token = obj?["token"]?.ToString();
                        if (string.IsNullOrEmpty(token))
                        {
                            EnqueueDebug("Response OK but token missing.");
                            throw new InvalidOperationException($"No token returned. Response:{responseContent}");
                        }
                        return token;
                    }
                    throw new InvalidOperationException("Empty response body when acquiring policy token.");
                }
                else if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    EnqueueDebug("202 Accepted received (async not supported)." );
                    throw new InvalidOperationException("Asynchronous policy token acquisition (202 Accepted) is not supported.");
                }
                else
                {
                    EnqueueDebug("Non-success status; will throw.");
                    throw new InvalidOperationException($"Policy token acquisition failed with {(int)response.StatusCode} {response.StatusCode}: {responseContent}");
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

        public object Clone()
        {
            return new AcquirePolicyTokenHandler(_cmdlet);
        }

        private void EnqueueDebug(string message)
        {
            try
            {
                _cmdlet?.DebugMessages?.Enqueue($"{LogPrefix} {message}");
            }
            catch { }
        }
    }
}
