// ----------------------------------------------------------------------------------
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// ----------------------------------------------------------------------------------

using Microsoft.WindowsAzure.Commands.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Commands.Common.Tests
{
    public class RequestHeaderScopeTests
    {
        private static readonly HttpRequestOptionsKey<IEnumerable<string>> ScopeKey =
            new HttpRequestOptionsKey<IEnumerable<string>>(RequestHeaderScope.RequestScopedHeadersKey);

        [Fact]
        public void MarkRequestScoped_MergesNamesCaseInsensitively_WithoutChangingHeaders()
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, "https://example.invalid/resource");
            request.Headers.Add("x-operation", "original");
            RequestHeaderScope.MarkRequestScoped(request, "x-operation");
            RequestHeaderScope.MarkRequestScoped(request, "X-OPERATION");
            RequestHeaderScope.MarkRequestScoped(request, "x-another-operation");

            Assert.True(request.Options.TryGetValue(ScopeKey, out var names));
            Assert.Equal(2, names.Count());
            Assert.Contains("x-operation", names);
            Assert.Contains("x-another-operation", names);
            Assert.Equal("original", Assert.Single(request.Headers.GetValues("x-operation")));
        }

        [Fact]
        public void MarkRequestScoped_PublishesImmutableSnapshots_ForShallowRetryClones()
        {
            using var write = new HttpRequestMessage();
            using var retry = new HttpRequestMessage();
            RequestHeaderScope.MarkRequestScoped(write, "x-operation");
            Assert.True(write.Options.TryGetValue(ScopeKey, out var originalNames));
            retry.Options.Set(ScopeKey, originalNames);

            RequestHeaderScope.MarkRequestScoped(retry, "x-retry-operation");

            Assert.Single(originalNames);
            Assert.True(write.Options.TryGetValue(ScopeKey, out var writeNames));
            Assert.Same(originalNames, writeNames);
            Assert.True(retry.Options.TryGetValue(ScopeKey, out var retryNames));
            Assert.NotSame(originalNames, retryNames);
            Assert.Equal(2, retryNames.Count());
            Assert.True(Assert.IsAssignableFrom<ICollection<string>>(retryNames).IsReadOnly);
        }

        [Fact]
        public void MarkRequestScoped_RejectsInvalidMetadata()
        {
            using var request = new HttpRequestMessage();
            request.Options.Set(new HttpRequestOptionsKey<object>(ScopeKey.Key), 42);

            Assert.Throws<InvalidOperationException>(() => RequestHeaderScope.MarkRequestScoped(request, "x-operation"));
        }

        [Fact]
        public void MarkRequestScoped_RejectsNullRequest()
        {
            Assert.Throws<ArgumentNullException>(() => RequestHeaderScope.MarkRequestScoped(null, "x-operation"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void MarkRequestScoped_RejectsEmptyName(string name)
        {
            using var request = new HttpRequestMessage();
            Assert.Throws<ArgumentException>(() => RequestHeaderScope.MarkRequestScoped(request, name));
        }

        [Theory]
        [InlineData("PUT")]
        [InlineData("POST")]
        [InlineData("PATCH")]
        [InlineData("DELETE")]
        public async Task StampPolicyTokenAsync_DeclaresSuccessfullyStampedHeader(string method)
        {
            using var request = NewRequest(method);
            RequestHeaderScope.MarkRequestScoped(request, "x-existing-operation");
            var tokenCalls = 0;
            using var tokenClient = TokenClient(() => tokenCalls++, "{\"token\":\"synthetic-token\"}");

            await new PolicyTokenAcquirer().StampPolicyTokenAsync(request, true, null, null, tokenClient, CancellationToken.None);

            Assert.Equal(1, tokenCalls);
            Assert.Equal("synthetic-token", Assert.Single(request.Headers.GetValues("x-ms-policy-external-evaluations")));
            Assert.True(request.Options.TryGetValue(ScopeKey, out var names));
            Assert.Contains("x-ms-policy-external-evaluations", names);
            Assert.Contains("x-existing-operation", names);
        }

        [Theory]
        [InlineData("GET", true)]
        [InlineData("HEAD", true)]
        [InlineData("PUT", false)]
        public async Task StampPolicyTokenAsync_SkippedRequestAddsNoScope(string method, bool acquire)
        {
            using var request = NewRequest(method);
            using var tokenClient = TokenClient(() => throw new InvalidOperationException("No token call expected."), "{}");

            await new PolicyTokenAcquirer().StampPolicyTokenAsync(request, acquire, null, null, tokenClient, CancellationToken.None);

            Assert.False(request.Options.TryGetValue(ScopeKey, out _));
            Assert.False(request.Headers.Contains("x-ms-policy-external-evaluations"));
        }

        [Fact]
        public async Task StampPolicyTokenAsync_EmptyTokenFailsWithoutAddingScope()
        {
            using var request = NewRequest("PUT");
            using var tokenClient = TokenClient(() => { }, "{\"token\":\"\"}");

            await Assert.ThrowsAsync<Microsoft.Azure.Commands.Common.Exceptions.AzPSCloudException>(() =>
                new PolicyTokenAcquirer().StampPolicyTokenAsync(request, true, null, null, tokenClient, CancellationToken.None));

            Assert.False(request.Options.TryGetValue(ScopeKey, out _));
            Assert.False(request.Headers.Contains("x-ms-policy-external-evaluations"));
        }

        private static HttpRequestMessage NewRequest(string method) =>
            new HttpRequestMessage(new HttpMethod(method),
                "https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg");

        private static HttpClient TokenClient(Action onCall, string body) =>
            new HttpClient(new MockTokenServerHandler((request, token) =>
            {
                onCall();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body)
                });
            }));
    }
}
