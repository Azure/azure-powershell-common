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
using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Core;
using Microsoft.Azure.Commands.ResourceManager.Common.Properties;
using Microsoft.Azure.Management.Internal.Resources.Utilities.Models;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Microsoft.Azure.Commands.ResourceManager.Common
{
    public static class AzureRmCmdletExtensions
    {
        public const int MAX_NUMBER_OF_TOKENS_ALLOWED_IN_AUX_HEADER = 3;
        public const string AUX_HEADER_NAME = "x-ms-authorization-auxiliary";
        public const string AUX_TOKEN_PREFIX = "Bearer";
        public const string AUX_TOKEN_APPEND_CHAR = ";";

        public static IDictionary<String, List<String>> GetAuxilaryAuthHeaderFromResourceIds(this AzureRMCmdlet cmdlet, List<String> resourceIds)
        {
            IDictionary<String, List<String>> auxHeader = null;

            //Get the subscriptions from the resource Ids
            var subscriptionIds = resourceIds.Select(rId => (new ResourceIdentifier(rId))?.Subscription)?.Distinct();

            //Checxk if we have access to all the subscriptions
            var subscriptionList = cmdlet.CheckAccessToSubscriptions(subscriptionIds);

            //get all the non default tenant ids for the subscriptions
            var nonDefaultTenantIds = subscriptionList?.Select(s => s.GetTenant())?.Distinct()?.Where(t => t != cmdlet.DefaultProfile.GetDefaultContext().Tenant.GetId().ToString());

            if ((nonDefaultTenantIds != null) && (nonDefaultTenantIds.Count() > 0))
            {
                // WE can only fill in tokens for 3 tennats in the aux header, if tehre are more tenants fail now
                if (nonDefaultTenantIds.Count() > MAX_NUMBER_OF_TOKENS_ALLOWED_IN_AUX_HEADER)
                {
                    throw new ArgumentException("Number of tenants (tenants other than the one in the current context), that the requested resources belongs to, exceeds maximum allowed number of " + MAX_NUMBER_OF_TOKENS_ALLOWED_IN_AUX_HEADER);
                }

                //get the tokens for each tenant and prepare the string in the following format :
                //"Header Value :: Bearer <auxiliary token1>;EncryptedBearer <auxiliary token2>; Bearer <auxiliary token3>"

                var tokens = nonDefaultTenantIds.Select(t => (new StringBuilder(AUX_TOKEN_PREFIX).Append(" ").Append(cmdlet.GetTokenForTenant(t)?.AccessToken))?.ToString())?.ConcatStrings(AUX_TOKEN_APPEND_CHAR);

                auxHeader = new Dictionary<String, List<String>>();

                List<string> headerValues = new List<string>(1);
                headerValues.Add(tokens);
                auxHeader.Add(AUX_HEADER_NAME, headerValues);
            }

            return auxHeader;
        }

        private static List<IAzureSubscription> CheckAccessToSubscriptions(this AzureRMCmdlet cmdlet, IEnumerable<string> subscriptions)
        {
            var subscriptionsNotInDefaultProfile = subscriptions.ToList().Except(cmdlet.DefaultProfile.Subscriptions.Select(s => s.GetId().ToString()).ToList());

            List<IAzureSubscription> subscriptionObjects = cmdlet.DefaultProfile.Subscriptions.Where(s => subscriptions.Contains(s.GetId().ToString())).ToList();
            if (subscriptionsNotInDefaultProfile.Any())
            {
                //So we didnt find some subscriptions in the default profile.. 
                //this does not mean that the user does not have access to the subs, it just menas that the local context did not have them
                //We gotta now call into the subscription RP and see if the user really does not have access to these subscriptions

                var result = Utilities.SubscriptionAndTenantHelper.GetTenantsForSubscriptions(subscriptionsNotInDefaultProfile.ToList(), cmdlet.DefaultProfile.GetDefaultContext());

                if (result.Count < subscriptionsNotInDefaultProfile.Count())
                {
                    var subscriptionsNotFoundAtAll = subscriptionsNotInDefaultProfile.ToList().Except(result.Keys);
                    //Found subscription(s) the user does not have acess to... throw exception
                    StringBuilder message = new StringBuilder();

                    message.Append(" The user does not have access to the following subscription(s) : ");
                    subscriptionsNotFoundAtAll.ForEach(s => message.Append(" " + s));
                    throw new AuthenticationException(message.ToString());
                }
                else
                {
                    subscriptionObjects.AddRange(result.Values);
                }
            }

            return subscriptionObjects;
        }

        private static IAccessToken GetTokenForTenant(this AzureRMCmdlet cmdlet, string tenantId)
        {
            return Utilities.SubscriptionAndTenantHelper.AcquireAccessToken(cmdlet.DefaultProfile.GetDefaultContext().Account,
                cmdlet.DefaultProfile?.DefaultContext.Environment,
                tenantId);
        }

        private static IAzureContext GetDefaultContext(this IAzureContextContainer profile)
        {
            if (profile?.DefaultContext?.Account == null)
            {
                throw new PSInvalidOperationException(Resources.RunConnectAccount);
            }

            return profile.DefaultContext;
        }


    }
}
