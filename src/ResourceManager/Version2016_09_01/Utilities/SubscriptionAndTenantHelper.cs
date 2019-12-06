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

using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.ResourceManager.Common.Paging;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.ResourceManager.Common.Utilities
{
    public class SubscriptionAndTenantHelper
    {
        internal static IAccessToken AcquireAccessToken(IAzureAccount account, IAzureEnvironment environment, string tenantId)
        {
            return AzureSession.Instance.AuthenticationFactory.Authenticate(
               account,
               environment,
               tenantId,
               null,
               ShowDialog.Never,
               null);
        }

        public static Dictionary<string, AzureSubscription> GetTenantsForSubscriptions<TClient>(List<string> subscriptionIds, IAzureContext defaultContext)
             where TClient : ServiceClient<TClient>
        {
            Dictionary<string, AzureSubscription> result = new Dictionary<string, AzureSubscription>();

            if (subscriptionIds != null && subscriptionIds.Count != 0)
            {
                //First get all the tenants, then get subscriptions in each tenant till we exhaust the subscriotions sent in
                //Or we exhaust the tenants
                List<AzureTenant> tenants = ListAccountTenants<TClient>(defaultContext);

                HashSet<string> subscriptionIdSet = new HashSet<string>(subscriptionIds);

                foreach (var tenant in tenants)
                {
                    if (subscriptionIdSet.Count <= 0)
                    {
                        break;
                    }

                    var tId = tenant.GetId().ToString();
                    var subscriptions = ListAllSubscriptionsForTenant<TClient>(defaultContext, tId);
                    
                    subscriptions?.ForEach((s) =>
                     {
                         var sId = s.GetId().ToString();
                         if (subscriptionIdSet.Contains(sId))
                         {
                             result.Add(sId, s);
                             subscriptionIdSet.Remove(sId);
                         }
                     }) ;
                }
            }

            return result;
        }

        private static List<AzureTenant> ListAccountTenants<TClient> (
            IAzureContext defaultContext) where TClient : ServiceClient<TClient>
        {
            List<AzureTenant> result = new List<AzureTenant>();
            var commonTenant = GetCommonTenant(defaultContext.Account);

            var commonTenantToken = AcquireAccessToken(
                defaultContext.Account,
                defaultContext.Environment,
                commonTenant);

            TClient subscriptionClient = null;
            try
            {
                subscriptionClient = AzureSession.Instance.ClientFactory.CreateCustomArmClient<TClient>(
                    defaultContext.Environment.GetEndpointAsUri(AzureEnvironment.Endpoint.ResourceManager),
                    new TokenCredentials(commonTenantToken.AccessToken) as ServiceClientCredentials,
                    AzureSession.Instance.ClientFactory.GetCustomHandlers());
                var tenantsProperty = typeof(TClient).GetProperty("Tenants");
                IPage<Object> tenants = tenantsProperty.GetType().GetMethod("List").Invoke(tenantsProperty, null) as IPage<Object>;
                if (tenants != null)
                {
                    result = new List<AzureTenant>();
                    tenants.ForEach((t) =>
                    {
                        result.Add(new AzureTenant { Id = t.GetType().GetProperty("TenantId").GetValue(t) as string, Directory = commonTenantToken.GetDomain() });
                    });
                }
            }
            finally
            {
                // In test mode, we are reusing the client since disposing of it will
                // fail some tests (due to HttpClient being null)
                if (subscriptionClient != null && !TestMockSupport.RunningMocked)
                {
                    subscriptionClient.Dispose();
                }
            }

            return result;
        }
        //return new GenericPageEnumerable<Subscription>(client.Subscriptions.List, client.Subscriptions.ListNext, ulong.MaxValue, 0);

        public static IEnumerable<AzureSubscription> ListAllSubscriptions<TSubscriptionClient>(TSubscriptionClient client, IAzureContext context)
        {
            var subscriptions = client.GetType().GetProperty("Subscriptions");
            var mInfoList = subscriptions.GetType().GetMethod("List");
            Func<IPage<Object>> list = Delegate.CreateDelegate(mInfoList.GetType(), mInfoList) as Func<IPage<Object>>;
            var mInfoListNext = subscriptions.GetType().GetMethod("ListNext");
            Func<string, IPage<Object>> listNext = Delegate.CreateDelegate(mInfoListNext.GetType(), mInfoListNext) as Func<string, IPage<Object>>;
            return new GenericPageEnumerable<Object>(list, listNext, ulong.MaxValue, 0).Select(s => ToAzureSubscription(s, context));
        }

        public static IEnumerable<AzureSubscription> ListAllSubscriptionsForTenant<TClient>(
            IAzureContext defaultContext,
            string tenantId) where TClient : ServiceClient<TClient>
        { 
            IAzureAccount account = defaultContext.Account;
            IAzureEnvironment environment = defaultContext.Environment;
            IAccessToken accessToken = null;
            try
            {
                accessToken = AcquireAccessToken(account, environment, tenantId);
            }
            catch (Exception e)
            {
                throw new AadAuthenticationFailedException("Could not find subscriptions", e);
            }

            TClient subscriptionClient = default(TClient);
            subscriptionClient = AzureSession.Instance.ClientFactory.CreateCustomArmClient<TClient>(
                    environment.GetEndpointAsUri(AzureEnvironment.Endpoint.ResourceManager),
                    new TokenCredentials(accessToken.AccessToken) as ServiceClientCredentials,
                    AzureSession.Instance.ClientFactory.GetCustomHandlers());

            AzureContext context = new AzureContext(defaultContext.Subscription, account, environment,
                                        CreateTenantFromString(tenantId, accessToken.TenantId));

            return ListAllSubscriptions(subscriptionClient, defaultContext);
        }

        private static string GetCommonTenant(IAzureAccount account)
        {
            string result = AzureEnvironmentConstants.CommonAdTenant;
            if (account.IsPropertySet(AzureAccount.Property.Tenants))
            {
                var candidate = account.GetTenants().FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    result = candidate;
                }
            }

            return result;
        }

        public static AzureSubscription ToAzureSubscription<TSubscription>(TSubscription other, IAzureContext context)
        {
            if(other != null && context != null)
            {
                var subscription = new AzureSubscription();
                subscription.SetAccount(context.Account != null ? context.Account.Id : null);
                subscription.SetEnvironment(context.Environment != null ? context.Environment.Name : EnvironmentName.AzureCloud);
                subscription.Id = other.GetType().GetProperty("SubscriptionId").GetValue(other) as string;
                subscription.Name = other.GetType().GetProperty("DisplayName").GetValue(other) as string;
                subscription.State = other.GetType().GetProperty("State").GetValue(other).ToString();
                subscription.SetProperty(AzureSubscription.Property.Tenants,
                    other.GetType().GetProperty("TenantId").GetValue(other) as string ?? context.Tenant.Id.ToString());
                return subscription;
            }
            return null;
        }

        private static AzureTenant CreateTenantFromString(string tenantOrDomain, string accessTokenTenantId)
        {
            AzureTenant result = new AzureTenant();
            Guid id;
            if (Guid.TryParse(tenantOrDomain, out id))
            {
                result.Id = tenantOrDomain;
            }
            else
            {
                result.Id = accessTokenTenantId;
                result.Directory = tenantOrDomain;
            }

            return result;
        }
    }
}
