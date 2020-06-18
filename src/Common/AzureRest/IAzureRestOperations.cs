using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Internal.Common
{
    /// <summary>
    /// AzureRest operations.
    /// </summary>
    public partial interface IAzureRestOperations
    {
        /// <summary>
        /// Support universal HTTP request
        /// </summary>
        /// <typeparam name="T">Type of return object</typeparam>
        /// <param name="method">Http Method</param>
        /// <param name="path">The path compoment in URL</param>
        /// <param name="queries">The queries compoment in URL</param>
        /// <param name="fragment">The fragment compoment in URL</param>
        /// <param name="content">The content in request body</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.Azure.CloudException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<AzureOperationResponse<T>> BeginHttpMessagesAsync<T>(HttpMethod method, string path, IDictionary<string, IList<string>> queries = null, string fragment = null, Object content = null, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// GET request to resource URI
        /// </summary>
        /// <typeparam name="T">Type of return object</typeparam>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.Azure.CloudException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<T> BeginHttpGetMessagesAsync<T>(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// DELETE request to resource URI
        /// </summary>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.Azure.CloudException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task BeginHttpDeleteMessagesAsync(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Update(PUT, POST, PATCH) request to resource URI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">Http Method</param>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="content">The content in request body</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.Azure.CloudException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<T> BeginHttpUpdateMessagesAsync<T>(HttpMethod method, string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        T GetResouce<T>(string resourceId, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        List<T> GetResouceList<T>(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        P GetResoucePage<P, T>(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken)) where P : IPage<T>;

        void DeleteResouce(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        T PutResouce<T>(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        T PostResouce<T>(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        T PatchResouce<T>(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /*Generic Section*/

        /// <summary>
        /// Support universal HTTP request
        /// </summary>
        /// <param name="method">Http Method</param>
        /// <param name="path">The path compoment in URL</param>
        /// <param name="queries">The queries compoment in URL</param>
        /// <param name="fragment">The fragment compoment in URL</param>
        /// <param name="content">The content in request body</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<AzureOperationResponse<string>> BeginHttpMessagesAsyncGeneric(HttpMethod method, string path, IDictionary<string, IList<string>> queries = null, string fragment = null, Object content = null, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// GET request to resource URI
        /// </summary>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<string> BeginHttpGetMessagesAsyncGeneric(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// DELETE request to resource URI
        /// </summary>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<string> BeginHttpDeleteMessagesAsyncGeneric(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Update(PUT, POST, PATCH) request to resource URI
        /// </summary>
        /// <param name="method">Http Method</param>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="content">The content in request body</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<string> BeginHttpUpdateMessagesAsyncGeneric(HttpMethod method, string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// GET request to resource URI
        /// </summary>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<AzureOperationResponse<string>> BeginHttpGetMessagesAsyncGenericFullResponse(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// DELETE request to resource URI
        /// </summary>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<AzureOperationResponse<string>> BeginHttpDeleteMessagesAsyncGenericFullResponse(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Update(PUT, POST, PATCH) request to resource URI
        /// </summary>
        /// <param name="method">Http Method</param>
        /// <param name="resourceUri">
        /// URI represents resource and correlated. Use the format
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}
        /// /subscriptions/{guid}/resourceGroups/{resource-group-name}/{resource-provider-namespace}/{resource-type}/{resource-name}/...
        /// </param>
        /// <param name="apiVersion">The API version to use for the operation.</param>
        /// <param name="content">The content in request body</param>
        /// <param name="customHeaders">The headers that will be added to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<AzureOperationResponse<string>> BeginHttpUpdateMessagesAsyncGenericFullResponse(HttpMethod method, string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string GetResourceGeneric(string resourceId, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string DeleteResourceGeneric(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string PutResourceGeneric(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string PostResourceGeneric(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string PatchResourceGeneric(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        AzureOperationResponse<string> GetResourceGenericFullResponse(string resourceId, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        AzureOperationResponse<string> DeleteResourceGenericFullResponse(string resourceUri, string apiVersion, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        AzureOperationResponse<string> PutResourceGenericFullResponse(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        AzureOperationResponse<string> PostResourceGenericFullResponse(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        AzureOperationResponse<string> PatchResourceGenericFullResponse(string resourceUri, string apiVersion, Object content, IDictionary<string, IList<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
