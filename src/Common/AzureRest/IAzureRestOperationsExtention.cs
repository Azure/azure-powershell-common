using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Internal.Common
{
    public partial interface IAzureRestOperations
    {
        Task<AzureOperationResponse<string>> BeginHttpMessagesAsyncGeneric(HttpMethod method, string path, IDictionary<string, IList<string>> queries = null, string fragment = null, Object content = null, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> BeginHttpGetMessagesAsyncGeneric(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> BeginHttpDeleteMessagesAsyncGeneric(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> BeginHttpUpdateMessagesAsyncGeneric(HttpMethod method, string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string GetResouceGeneric(string resourceId, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string DeleteResouceGeneric(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string PutResouceGeneric(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string PostResouceGeneric(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string PatchResouceGeneric(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}