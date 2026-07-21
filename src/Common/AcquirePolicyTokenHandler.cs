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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Delegating handler to acquire an Azure Policy token for the change safety feature
    /// and attach it to outgoing write requests.
    /// Activated when user specifies -AcquirePolicyToken or -ChangeReference.
    /// </summary>
    public class AcquirePolicyTokenHandler : DelegatingHandler, ICloneable
    {
        private readonly bool _shouldAcquire;
        private readonly string _changeReference;
        private readonly ConcurrentQueue<string> _debugMessages;
        private readonly HttpClient _tokenHttpClient;

        private static readonly PolicyTokenAcquirer Acquirer = new PolicyTokenAcquirer();

        /// <summary>
        /// Creates a handler with explicit parameter values (no cmdlet reference needed).
        /// </summary>
        /// <param name="shouldAcquire">Whether the user requested policy token acquisition.</param>
        /// <param name="changeReference">The change reference ID, or null if not specified.</param>
        /// <param name="debugMessages">Queue for debug messages, or null.</param>
        /// <param name="tokenHttpClient">Optional HttpClient for the token API call (for testing).</param>
        public AcquirePolicyTokenHandler(
            bool shouldAcquire,
            string changeReference,
            ConcurrentQueue<string> debugMessages,
            HttpClient tokenHttpClient = null)
        {
            _shouldAcquire = shouldAcquire;
            _changeReference = changeReference;
            _debugMessages = debugMessages;
            _tokenHttpClient = tokenHttpClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Acquirer.StampPolicyTokenAsync(request, _shouldAcquire, _changeReference, _debugMessages, _tokenHttpClient, cancellationToken).ConfigureAwait(false);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public object Clone()
        {
            return new AcquirePolicyTokenHandler(_shouldAcquire, _changeReference, _debugMessages, _tokenHttpClient);
        }
    }
}
