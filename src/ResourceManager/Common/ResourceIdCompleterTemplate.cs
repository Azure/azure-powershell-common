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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Rest.Azure.OData;
using Microsoft.Rest;
using Microsoft.Rest.Azure;

namespace Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ResourceIdCompleterAttributeTemplate : ArgumentCompleterAttribute
    {
        private static readonly object Lock = new object();

        /// <summary>
        /// Consturctor
        /// </summary>
        /// <param name="resourceType">Azure recource type</param>
        public ResourceIdCompleterAttributeTemplate(string resourceType)
            : base(CreateScriptBlock(resourceType))
        {}

        public static ScriptBlock CreateScriptBlock(string resourceType)
        {
            string script = "param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)\n" +
                $"$resourceType = \"{resourceType}\"\n" +
                "$resourceIds = [Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters.ResourceIdCompleterAttribute]::GetResourceIds($resourceType)\n" +
                "$resourceIds | Where-Object { $_ -Like \"*$wordToComplete*\" } | Sort-Object | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }";
            var scriptBlock = ScriptBlock.Create(script);
            return scriptBlock;
        }

        private class CacheItem
        {
            public DateTime Timestamp { get; set; }
            public IEnumerable<string> ResourceInfoList { get; set; }
        }

        private static readonly IDictionary<int, CacheItem> Cache = new Dictionary<int, CacheItem>();

        public static TimeSpan TimeToUpdate { get; set; } = TimeSpan.FromMinutes(5);

        public static TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public static IEnumerable<string> GetResourceIds<TClient, TFilter>(string resourceType, Expression<Func<TFilter, bool>> filter) where TClient : ServiceClient<TClient>
        {
            lock (Lock)
            {
                var context = AzureRmProfileProvider.Instance.Profile.DefaultContext;
                var contextHash = HashContext(context, resourceType);
                var cacheItem = Cache.ContainsKey(contextHash) ? Cache[contextHash] : null;

                if (cacheItem != null && DateTime.Now.Subtract(cacheItem.Timestamp).CompareTo(TimeToUpdate) < 0)
                {
                    return Cache[contextHash].ResourceInfoList;
                }

                var client = AzureSession.Instance.ClientFactory.CreateArmClient<TClient>(context, AzureEnvironment.Endpoint.ResourceManager);

                var odata = new ODataQuery<TFilter>(filter);

                IEnumerable<string> resourceInfoList = new List<string>();

                try
                {
                    using (var cancelSource = new CancellationTokenSource())
                    {
                        var resources = client.GetType().GetProperty("Resources").GetValue(client);
                        Task<IPage<Object>> task = (Task<IPage<Object>>)resources.GetType().GetMethod("ListAsync").Invoke(resources, new object [] {odata, cancelSource.Token });

                        if (!task.Wait(RequestTimeout))
                        {
                            cancelSource.Cancel();
                            return resourceInfoList;
                        }

                        var result = task.Result;
                        resourceInfoList = result
                            .Select(r => r.GetType().GetProperty("Id").GetValue(r) as string)
                            .ToList();
                    }
                }
                catch (Exception)
                {
                    return resourceInfoList;
                }

                if (cacheItem != null)
                {
                    cacheItem.Timestamp = DateTime.Now;
                    cacheItem.ResourceInfoList = resourceInfoList;
                    Cache[contextHash] = cacheItem;
                }
                else
                {
                    Cache.Add(contextHash, new CacheItem
                    {
                        Timestamp = DateTime.Now,
                        ResourceInfoList = resourceInfoList
                    });
                }

                return resourceInfoList;
            }
        }

        private static int HashContext(IAzureContext context, string resourceType)
        {
            return (context.Account.Id + context.Environment.Name + context.Subscription.Id + context.Tenant.Id + resourceType).GetHashCode();
        }
    }
}
