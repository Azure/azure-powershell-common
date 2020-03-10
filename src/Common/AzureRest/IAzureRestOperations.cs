using Microsoft.Rest.Azure;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.Common.AzureRest
{
    public partial interface IAzureRestOperations
    {

        Task<AzureOperationResponse<T>> BeginHttpMessagesAsync<T>(HttpMethod method, string path, Dictionary<string, List<string>> queries = null, string fragment = null, string content = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default);

        Task<T> GetByResouceIdAsync<T>(string resourceId, string apiVersion, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        T GetByResouceId<T>(string resourceId, string apiVersion, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<T>> GetResouceListAsync<T>(string resourceUri, string apiVersion, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        List<T> GetResouceList<T>(string resourceUri, string apiVersion, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        Task<IPage<T>> GetResoucePageAsync<T>(string resourceUri, string apiVersion, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        IPage<T> GetResoucePage<T>(string resourceUri, string apiVersion, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
