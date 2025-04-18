﻿// ----------------------------------------------------------------------------------
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
using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;
using Microsoft.Azure.Commands.Common.Authentication.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.Paging;
using Microsoft.Azure.Graph.RBAC.Version1_6_20190326;
using Microsoft.Azure.Graph.RBAC.Version1_6_20190326.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using ProjectResources = Microsoft.Azure.Commands.Common.Graph.RBAC.Properties.Resources;

namespace Microsoft.Azure.Graph.RBAC.Version1_6_20190326.ActiveDirectory
{
    public class ActiveDirectoryClient
    {
        public GraphRbacManagementClient GraphClient { get; private set; }

        /// <summary>
        /// Creates new ActiveDirectoryClient using Azure Subscription.
        /// </summary>
        /// <param name="context">The Azure context</param>
        /// <param name="cmdletContext">The cmdlet context</param>
        public ActiveDirectoryClient(IAzureContext context, ICmdletContext cmdletContext)
        {
            GraphClient = AzureSession.Instance.ClientFactory.CreateArmClient<GraphRbacManagementClient>(
                context, AzureEnvironment.Endpoint.Graph, cmdletContext);

            GraphClient.TenantID = context.Tenant.Id.ToString();
        }

        /// <summary>
        /// Creates new ActiveDirectoryClient using Azure Subscription.
        /// </summary>
        /// <param name="context">The Azure context</param>
        public ActiveDirectoryClient(IAzureContext context)
        {
            GraphClient = AzureSession.Instance.ClientFactory.CreateArmClient<GraphRbacManagementClient>(
                context, AzureEnvironment.Endpoint.Graph);

            GraphClient.TenantID = context.Tenant.Id.ToString();
        }

        public PSADObject GetADObject(ADObjectFilterOptions options)
        {
            PSADObject result = null;

            Debug.Assert(options != null);

            if (IsSet(options.Mail, options.UPN, options.Id))
            {
                result = FilterUsers(options).FirstOrDefault();
            }

            if (result == null && IsSet(options.SPN, options.Id))
            {
                result = FilterServicePrincipals(options).FirstOrDefault();
            }

            if (result == null && IsSet(options.Mail, options.Id))
            {
                result = FilterGroups(options).FirstOrDefault();
            }

            return result;
        }

        private static bool IsSet(params string[] strings)
        {
            return strings.Any(s => !string.IsNullOrEmpty(s));
        }

        private static string Normalize(string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        public PSADServicePrincipal GetServicePrincipalByObjectId(string objectId)
        {
            PSADServicePrincipal servicePrincipal = null;
            try
            {
                servicePrincipal = GraphClient.ServicePrincipals.Get(objectId).ToPSADServicePrincipal();
            }
            catch { /* The service principal does not exist, ignore the exception. */ }

            return servicePrincipal;
        }

        public PSADServicePrincipal GetServicePrincipalBySPN(string spn)
        {
            PSADServicePrincipal servicePrincipal = null;
            try
            {
                var odataQuery = new Rest.Azure.OData.ODataQuery<ServicePrincipal>(s => s.ServicePrincipalNames.Contains(spn));
                servicePrincipal = GraphClient.ServicePrincipals.List(odataQuery.ToString()).FirstOrDefault()?.ToPSADServicePrincipal();
            }
            catch { /* The service principal does not exist, ignore the exception. */ }

            return servicePrincipal;
        }

        public IEnumerable<PSADServicePrincipal> FilterServicePrincipals(Rest.Azure.OData.ODataQuery<ServicePrincipal> odataQuery, ulong first = ulong.MaxValue, ulong skip = 0)
        {
            return new GenericPageEnumerable<ServicePrincipal>(
                delegate ()
                {
                    return GraphClient.ServicePrincipals.List(odataQuery);
                }, GraphClient.ServicePrincipals.ListNext, first, skip).Select(s => s.ToPSADServicePrincipal());
        }

        public IEnumerable<PSADServicePrincipal> FilterServicePrincipals(ADObjectFilterOptions options, ulong first = ulong.MaxValue, ulong skip = 0)
        {
            List<PSADServicePrincipal> servicePrincipals = new List<PSADServicePrincipal>();
            ServicePrincipal servicePrincipal = null;

            if (!string.IsNullOrEmpty(options.Id))
            {
                try
                {
                    servicePrincipal = GraphClient.ServicePrincipals.Get(options.Id);
                }
                catch {  /* The user does not exist, ignore the exception. */ }

                if (servicePrincipal != null)
                {
                    servicePrincipals.Add(servicePrincipal.ToPSADServicePrincipal());
                }
            }
            else if (!string.IsNullOrEmpty(options.SPN))
            {
                try
                {
                    var odataQuery = new Rest.Azure.OData.ODataQuery<ServicePrincipal>(s => s.ServicePrincipalNames.Contains(options.SPN));
                    servicePrincipal = GraphClient.ServicePrincipals.List(odataQuery.ToString()).FirstOrDefault();
                }
                catch {  /* The user does not exist, ignore the exception. */ }

                if (servicePrincipal != null)
                {
                    servicePrincipals.Add(servicePrincipal.ToPSADServicePrincipal());
                }
            }
            else
            {
                Rest.Azure.OData.ODataQuery<ServicePrincipal> odataQuery = null;
                if (!string.IsNullOrEmpty(options.SearchString) && options.SearchString.EndsWith("*"))
                {
                    options.SearchString = options.SearchString.TrimEnd('*');
                    odataQuery = new Rest.Azure.OData.ODataQuery<ServicePrincipal>(s => s.DisplayName.StartsWith(options.SearchString));
                }
                else
                {
                    odataQuery = new Rest.Azure.OData.ODataQuery<ServicePrincipal>(s => s.DisplayName == options.SearchString);
                }

                return FilterServicePrincipals(odataQuery, first, skip);
            }

            return servicePrincipals;
        }

        public PSADUser CreateUser(UserCreateParameters userCreateParam)
        {
            return GraphClient.Users.Create(userCreateParam).ToPSADUser();
        }

        public PSADUser UpdateUser(string upnOrObjectId, UserUpdateParameters userUpdateParam)
        {
            GraphClient.Users.Update(upnOrObjectId, userUpdateParam);
            return GraphClient.Users.Get(upnOrObjectId).ToPSADUser();
        }

        public void RemoveUser(string upnOrObjectId)
        {
            GraphClient.Users.Delete(upnOrObjectId);
        }

        public IEnumerable<PSADUser> FilterUsers(ADObjectFilterOptions options, ulong first = ulong.MaxValue, ulong skip = 0)
        {
            if (!string.IsNullOrEmpty(options.Id))
            {
                User user = null;
                try
                {
                    user = GraphClient.Users.Get(Normalize(options.Id));
                }
                catch {  /* The user does not exist, ignore the exception. */ }

                if (user != null)
                {
                    return new List<PSADUser> { user.ToPSADUser() };
                }
            }
            else if (!string.IsNullOrEmpty(options.UPN) || !string.IsNullOrEmpty(options.Mail))
            {
                IPage<User> result = null;
                try
                {
                    string upnOrMail = Normalize(options.UPN) ?? Normalize(options.Mail);
                    var odataQuery = new Rest.Azure.OData.ODataQuery<User>();
                    if (!string.IsNullOrEmpty(options.UPN))
                    {
                        odataQuery.SetFilter(u => u.UserPrincipalName == upnOrMail);
                    }
                    else
                    {
                        odataQuery.SetFilter(u => u.Mail == upnOrMail);
                    }
                    result = GraphClient.Users.List(odataQuery);
                }
                catch {  /* The user does not exist, ignore the exception. */ }

                if (result != null)
                {
                    return result.Select(u => u.ToPSADUser());
                }
            }
            else
            {
                Rest.Azure.OData.ODataQuery<User> odataQuery = null;
                if (!string.IsNullOrEmpty(options.SearchString) && options.SearchString.EndsWith("*"))
                {
                    options.SearchString = options.SearchString.TrimEnd('*');
                    odataQuery = new Rest.Azure.OData.ODataQuery<User>(u => u.DisplayName.StartsWith(options.SearchString));
                }
                else
                {
                    odataQuery = new Rest.Azure.OData.ODataQuery<User>(u => u.DisplayName == options.SearchString);
                }

                return new GenericPageEnumerable<User>(
                    delegate ()
                    {
                        return GraphClient.Users.List(odataQuery.ToString());
                    }, GraphClient.Users.ListNext, first, skip).Select(u => u.ToPSADUser());
            }

            return new List<PSADUser>();
        }

        public IEnumerable<PSADUser> FilterUsers()
        {
            return FilterUsers(new ADObjectFilterOptions());
        }

        public List<PSADObject> ListUserGroups(string principal)
        {
            List<PSADObject> result = new List<PSADObject>();
            Guid objectId = GetObjectId(new ADObjectFilterOptions { UPN = principal });
            PSADObject user = GetADObject(new ADObjectFilterOptions { Id = objectId.ToString() });
            var groupsIds = GraphClient.Users.GetMemberGroups(objectId.ToString(), new UserGetMemberGroupsParameters());
            var groupsResult = GraphClient.Objects.GetObjectsByObjectIds(new GetObjectsParameters { ObjectIds = groupsIds.ToList() });
            result.AddRange(groupsResult.Select(g => g.ToPSADGroup()));

            return result;
        }

        /// <summary>
        /// The graph getobjectsbyObjectId API supports 1000 objectIds per call.
        /// Due to this we are batching objectIds by chunk size of 1000 per APi call if it exceeds 1000
        /// </summary>
        public List<PSADObject> GetObjectsByObjectId(List<string> objectIds)
        {
            List<PSADObject> result = new List<PSADObject>();
            IPage<DirectoryObject> adObjects;
            int objectIdBatchCount;
            for(int i=0; i<objectIds.Count; i+=1000)
            {
                if((i+1000) > objectIds.Count){
                    objectIdBatchCount = objectIds.Count - i;
                }
                else{
                    objectIdBatchCount = 1000;
                }
                adObjects = GraphClient.Objects.GetObjectsByObjectIds(new GetObjectsParameters { ObjectIds = objectIds.GetRange(i, objectIdBatchCount), IncludeDirectoryObjectReferences = true });
                result.AddRange(adObjects.Select(o => o.ToPSADObject()));
            }
            return result;
        }

        public PSADGroup GetGroupByDisplayName(string displayName)
        {
            var group = FilterGroups(new ADObjectFilterOptions() { SearchString = displayName });
            if (group.Count() > 1)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.MultipleGroupsWithDisplayNameFound, displayName));
            }

            if (group.Count() == 0)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.GroupWithDisplayNameDoesntExist, displayName));
            }

            return group.FirstOrDefault();
        }

        public IEnumerable<PSADGroup> FilterGroups(ADObjectFilterOptions options, ulong first = ulong.MaxValue, ulong skip = 0)
        {
            if (!string.IsNullOrEmpty(options.Id))
            {
                try
                {
                    // use GetObjectsByObjectId to handle Redirects in the CSP scenario
                    PSADGroup group = this.GetObjectsByObjectId(new List<string> { options.Id }).FirstOrDefault() as PSADGroup;
                    if (group != null)
                    {
                        return new List<PSADGroup> { group };
                    }
                }
                catch {  /* The group does not exist, ignore the exception */ }
            }
            else
            {
                Rest.Azure.OData.ODataQuery<ADGroup> odataQuery = null;
                if (options.Mail != null)
                {
                    odataQuery = new Rest.Azure.OData.ODataQuery<ADGroup>(g => g.Mail == options.Mail);
                }
                else
                {
                    if (!string.IsNullOrEmpty(options.SearchString) && options.SearchString.EndsWith("*"))
                    {
                        options.SearchString = options.SearchString.TrimEnd('*');
                        odataQuery = new Rest.Azure.OData.ODataQuery<ADGroup>(g => g.DisplayName.StartsWith(options.SearchString));
                    }
                    else
                    {
                        odataQuery = new Rest.Azure.OData.ODataQuery<ADGroup>(g => g.DisplayName == options.SearchString);
                    }
                }

                return new GenericPageEnumerable<ADGroup>(
                    delegate ()
                    {
                        return GraphClient.Groups.List(odataQuery);
                    }, GraphClient.Groups.ListNext, first, skip).Select(g => g.ToPSADGroup());
            }

            return new List<PSADGroup>();
        }

        public IEnumerable<PSADGroup> FilterGroups()
        {
            return FilterGroups(new ADObjectFilterOptions());
        }

        public PSADGroup CreateGroup(GroupCreateParameters groupCreateParams)
        {
            return GraphClient.Groups.Create(groupCreateParams).ToPSADGroup();
        }

        public void RemoveGroup(string groupObjectId)
        {
            GraphClient.Groups.Delete(groupObjectId);
        }

        public IEnumerable<PSADObject> GetGroupMembers(ADObjectFilterOptions options, ulong first = ulong.MaxValue, ulong skip = 0)
        {
            return new GenericPageEnumerable<DirectoryObject>(
                delegate ()
                {
                    return GraphClient.Groups.GetGroupMembers(options.Id);
                }, GraphClient.Groups.GetGroupMembersNext, first, skip).Select(m => m.ToPSADObject());
        }

        public void AddGroupMember(string groupObjectId, GroupAddMemberParameters groupAddMemberParams)
        {
            GraphClient.Groups.AddMember(groupObjectId, groupAddMemberParams);
        }

        public void RemoveGroupMember(string groupObjectId, string memberObjectId)
        {
            GraphClient.Groups.RemoveMember(groupObjectId, memberObjectId);
        }

        public Guid GetObjectId(ADObjectFilterOptions options)
        {
            Guid principalId;
            if (options != null && options.Id != null
                && Guid.TryParse(options.Id, out principalId))
            {
                // do nothing, we have parsed the guid
            }
            else
            {
                PSADObject adObj = GetADObject(options);

                if (adObj == null)
                {
                    throw new KeyNotFoundException("The provided information does not map to an AD object id.");
                }

                principalId = adObj.Id;
            }

            return principalId;
        }

        public string GetAdfsObjectId(ADObjectFilterOptions options)
        {
            string principalId = null;
            if (options != null && options.Id != null)
            {
                // do nothing, we have parsed the guid
            }
            else
            {
                PSADObject adObj = GetADObject(options);

                if (adObj == null)
                {
                    throw new KeyNotFoundException("The provided information does not map to an AD object id.");
                }

                principalId = adObj.AdfsId;
            }

            return principalId;
        }
		
        public void UpdateApplication(Guid appObjectId, ApplicationUpdateParameters parameters)
        {
            GraphClient.Applications.Patch(appObjectId.ToString(), parameters);
        }

        public PSADApplication CreateApplication(CreatePSApplicationParameters createParameters)
        {
            IList<PasswordCredential> passwordCredentials = createParameters.PasswordCredentials != null
                ? createParameters.PasswordCredentials.Select(psCredential => psCredential.ToGraphPasswordCredential()).ToList()
                : null;

            IList<KeyCredential> keyCredentials = createParameters.KeyCredentials != null
                ? createParameters.KeyCredentials.Select(psCredential => psCredential.ToGraphKeyCredential()).ToList()
                : null;

            ApplicationCreateParameters graphParameters = new ApplicationCreateParameters
            {
                DisplayName = createParameters.DisplayName,
                Homepage = createParameters.HomePage,
                IdentifierUris = createParameters.IdentifierUris,
                ReplyUrls = createParameters.ReplyUrls,
                AvailableToOtherTenants = createParameters.AvailableToOtherTenants,
                PasswordCredentials = passwordCredentials,
                KeyCredentials = keyCredentials
            };

            try
            {
                return GraphClient.Applications.Create(graphParameters).ToPSADApplication();
            }
            catch (GraphErrorException ce)
            {
                if (ce.Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    User currentUser = GraphClient.SignedInUser.Get();
                    if (currentUser != null && string.Equals(currentUser.UserType, "Guest", StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new InvalidOperationException(ProjectResources.CreateApplicationNotAllowedGuestUser);
                    }
                }

                throw;
            }
        }

        private void ValidateKeyCredential(KeyCredential credential)
        {
            if (credential == null || string.IsNullOrEmpty(credential.KeyId) || string.IsNullOrEmpty(credential.Value) ||
                string.IsNullOrEmpty(credential.Type) || string.IsNullOrEmpty(credential.Usage) || credential.StartDate == null || credential.EndDate == null)
            {
                throw new InvalidOperationException(ProjectResources.KeyCredentialNotValid);
            }
        }

        private void ValidatePasswordCredential(PasswordCredential credential)
        {
            if (credential == null || string.IsNullOrEmpty(credential.KeyId) || string.IsNullOrEmpty(credential.Value) ||
                credential.StartDate == null || credential.EndDate == null)
            {
                throw new InvalidOperationException(ProjectResources.PasswordCredentialNotValid);
            }
        }

        private List<KeyCredential> GetAppKeyCredentials(Guid appObjectId)
        {
            return GraphClient.Applications.ListKeyCredentials(appObjectId.ToString()).ToList();
        }

        private List<PasswordCredential> GetAppPasswordCredentials(Guid appObjectId)
        {
            return GraphClient.Applications.ListPasswordCredentials(appObjectId.ToString()).ToList();
        }

        private void PatchAppKeyCredentials(Guid appObjectId, List<KeyCredential> keyCredentails)
        {
            if (keyCredentails == null)
            {
                keyCredentails = new List<KeyCredential>();
            }

            GraphClient.Applications.UpdateKeyCredentials(appObjectId.ToString(), new KeyCredentialsUpdateParameters(keyCredentails));
        }

        private void PatchAppPasswordCredentials(Guid appObjectId, List<PasswordCredential> passwordCredentials)
        {
            if (passwordCredentials == null)
            {
                passwordCredentials = new List<PasswordCredential>();
            }

            GraphClient.Applications.UpdatePasswordCredentials(appObjectId.ToString(), new PasswordCredentialsUpdateParameters(passwordCredentials));
        }

        public PSADCredential CreateAppKeyCredential(Guid appObjectId, KeyCredential credential)
        {
            ValidateKeyCredential(credential);

            var keyCredsList = GetAppKeyCredentials(appObjectId);

            // Add new KeyCredential to existing KeyCredential list
            keyCredsList.Add(credential);

            PatchAppKeyCredentials(appObjectId, keyCredsList);

            return credential.ToPSADCredential();
        }

        public PSADCredential CreateAppPasswordCredential(Guid appObjectId, PasswordCredential credential)
        {
            ValidatePasswordCredential(credential);

            var passwordCredsList = GetAppPasswordCredentials(appObjectId);

            // Add new PasswordCredential to existing KeyCredential list
            passwordCredsList.Add(credential);

            PatchAppPasswordCredentials(appObjectId, passwordCredsList);

            return credential.ToPSADCredential();
        }

        public List<PSADCredential> GetAppCredentials(Guid appObjectId)
        {
            List<PSADCredential> CredentialList = new List<PSADCredential>();

            var keyCredsList = GetAppKeyCredentials(appObjectId);
            CredentialList.AddRange(keyCredsList.Select(kc => kc.ToPSADCredential()));

            var passwordCredsList = GetAppPasswordCredentials(appObjectId);
            CredentialList.AddRange(passwordCredsList.Select(pc => pc.ToPSADCredential()));

            return CredentialList;
        }


        public void RemoveAppCredentialByKeyId(Guid appObjectId, Guid keyId)
        {
            var keyCredsList = GetAppKeyCredentials(appObjectId);

            var toBeDeletedKeyCred = keyCredsList.Find(kc => Guid.Parse(kc.KeyId) == keyId);

            if (toBeDeletedKeyCred != null)
            {
                keyCredsList.Remove(toBeDeletedKeyCred);
                PatchAppKeyCredentials(appObjectId, keyCredsList);
            }
            else
            {
                var passwordCredsList = GetAppPasswordCredentials(appObjectId);
                var toBeDeletedPasswwordCred = passwordCredsList.Find(pc => Guid.Parse(pc.KeyId) == keyId);

                if (toBeDeletedPasswwordCred != null)
                {
                    passwordCredsList.Remove(toBeDeletedPasswwordCred);
                    PatchAppPasswordCredentials(appObjectId, passwordCredsList);
                }
            }
        }

        public void RemoveAllAppCredentials(Guid appObjectId)
        {
            PatchAppKeyCredentials(appObjectId, keyCredentails: null);
            PatchAppPasswordCredentials(appObjectId, passwordCredentials: null);
        }

        public Guid GetAppObjectIdFromApplicationId(Guid applicationId)
        {
            var applicationIdString = applicationId.ToString();
            var odataQueryFilter = new Rest.Azure.OData.ODataQuery<Application>(a => a.AppId == applicationIdString);
            var app = GetApplicationWithFilters(odataQueryFilter).SingleOrDefault();
            if (app == null)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.ApplicationWithAppIdDoesntExist, applicationId));
            }
            return app.Id;
        }

        public Guid GetAppObjectIdFromDisplayName(string displayName)
        {
            var odataQueryFilter = new Rest.Azure.OData.ODataQuery<Application>(a => a.DisplayName == displayName);
            var app = GetApplicationWithFilters(odataQueryFilter);
            if (app == null || app.FirstOrDefault() == null)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.ApplicationWithDisplayNameDoesntExist, displayName));
            }

            if (app.Count() > 1)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.MultipleApplicationsWithDisplayNameFound, displayName));
            }

            return app.FirstOrDefault().Id;
        }

        public Guid GetUserObjectIdFromDisplayName(string displayName)
        {
            var odataQueryFilter = new Rest.Azure.OData.ODataQuery<User>(u => u.DisplayName == displayName);
            var user = GraphClient.Users.List(odataQueryFilter.ToString());
            if (user == null || user.FirstOrDefault() == null)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.UserWithDisplayNameDoesntExist, displayName));
            }

            if (user.Count() > 1)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.MultipleUsersWithDisplayNameFound, displayName));
            }

            return new Guid(user.FirstOrDefault().ObjectId);
        }

        private List<KeyCredential> GetSpKeyCredentials(Guid spObjectId)
        {
            return GraphClient.ServicePrincipals.ListKeyCredentials(spObjectId.ToString()).ToList();
        }

        private List<PasswordCredential> GetSpPasswordCredentials(Guid spObjectId)
        {
            return GraphClient.ServicePrincipals.ListPasswordCredentials(spObjectId.ToString()).ToList();
        }

        private void PatchSpKeyCredentials(Guid spObjectId, List<KeyCredential> keyCredentails)
        {
            if (keyCredentails == null)
            {
                keyCredentails = new List<KeyCredential>();
            }

            GraphClient.ServicePrincipals.UpdateKeyCredentials(spObjectId.ToString(), new KeyCredentialsUpdateParameters(keyCredentails));
        }

        private void PatchSpPasswordCredentials(Guid spObjectId, List<PasswordCredential> passwordCredentials)
        {
            if (passwordCredentials == null)
            {
                passwordCredentials = new List<PasswordCredential>();
            }

            GraphClient.ServicePrincipals.UpdatePasswordCredentials(spObjectId.ToString(), new PasswordCredentialsUpdateParameters(passwordCredentials));
        }


        public PSADCredential CreateSpKeyCredential(Guid spObjectId, KeyCredential credential)
        {
            ValidateKeyCredential(credential);

            var keyCredsList = GetSpKeyCredentials(spObjectId);

            // Add new KeyCredential to existing KeyCredential list
            keyCredsList.Add(credential);

            PatchSpKeyCredentials(spObjectId, keyCredsList);

            return credential.ToPSADCredential();
        }

        public PSADCredential CreateSpPasswordCredential(Guid spObjectId, PasswordCredential credential)
        {
            ValidatePasswordCredential(credential);

            var passwordCredsList = GetSpPasswordCredentials(spObjectId);

            // Add new PasswordCredential to existing KeyCredential list
            passwordCredsList.Add(credential);

            PatchSpPasswordCredentials(spObjectId, passwordCredsList);

            return credential.ToPSADCredential();
        }

        public List<PSADCredential> GetSpCredentials(Guid spObjectId)
        {
            List<PSADCredential> CredentialList = new List<PSADCredential>();

            var keyCredsList = GetSpKeyCredentials(spObjectId);
            CredentialList.AddRange(keyCredsList.Select(kc => kc.ToPSADCredential()));

            var passwordCredsList = GetSpPasswordCredentials(spObjectId);
            CredentialList.AddRange(passwordCredsList.Select(pc => pc.ToPSADCredential()));

            return CredentialList;
        }

        public void RemoveSpCredentialByKeyId(Guid spObjectId, Guid keyId)
        {
            var keyCredsList = GetSpKeyCredentials(spObjectId);

            var toBeDeletedKeyCred = keyCredsList.Find(kc => Guid.Parse(kc.KeyId) == keyId);

            if (toBeDeletedKeyCred != null)
            {
                keyCredsList.Remove(toBeDeletedKeyCred);
                PatchSpKeyCredentials(spObjectId, keyCredsList);
            }
            else
            {
                var passwordCredsList = GetSpPasswordCredentials(spObjectId);
                var toBeDeletedPasswwordCred = passwordCredsList.Find(pc => Guid.Parse(pc.KeyId) == keyId);

                if (toBeDeletedPasswwordCred != null)
                {
                    passwordCredsList.Remove(toBeDeletedPasswwordCred);
                    PatchSpPasswordCredentials(spObjectId, passwordCredsList);
                }
            }
        }

        public void RemoveAllSpCredentials(Guid spObjectId)
        {
            PatchSpKeyCredentials(spObjectId, keyCredentails: null);
            PatchSpPasswordCredentials(spObjectId, passwordCredentials: null);
        }

        public Guid GetObjectIdFromUPN(string upn)
        {
            var odataQueryFilter = new Rest.Azure.OData.ODataQuery<User>(s => s.UserPrincipalName == upn);
            var user = GraphClient.Users.List(odataQueryFilter.ToString()).SingleOrDefault();
            if (user == null)
            {
                throw new InvalidOperationException(String.Format(ProjectResources.UserWithUPNDoesntExist, upn));
            }

            return new Guid(user.ObjectId);
        }

        public Guid GetObjectIdFromSPN(string spn)
        {
            var odataQueryFilter = new Rest.Azure.OData.ODataQuery<ServicePrincipal>(s => s.ServicePrincipalNames.Contains(spn));
            var sp = GraphClient.ServicePrincipals.List(odataQueryFilter.ToString()).SingleOrDefault();
            if (sp == null)
            {
                throw new InvalidOperationException(String.Format(ProjectResources.ServicePrincipalWithSPNDoesntExist, spn));
            }

            return new Guid(sp.ObjectId);
        }

        public Guid GetObjectIdFromServicePrincipalDisplayName(string displayName)
        {
            var odataQueryFilter = new Rest.Azure.OData.ODataQuery<ServicePrincipal>(s => s.DisplayName == displayName);
            var sp = GraphClient.ServicePrincipals.List(odataQueryFilter.ToString());
            if (sp == null || sp.FirstOrDefault() == null)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.ServicePrincipalWithDisplayNameDoesntExist, displayName));
            }

            if (sp.Count() > 1)
            {
                throw new InvalidOperationException(string.Format(ProjectResources.MultipleServicePrincipalsWithDisplayNameFound, displayName));
            }

            return new Guid(sp.FirstOrDefault().ObjectId);
        }

        public void RemoveApplication(Guid applicationObjectId)
        {
            GraphClient.Applications.Delete(applicationObjectId.ToString());
        }

        public PSADApplication GetApplication(Guid applicationObjectId)
        {
            return GraphClient.Applications.Get(applicationObjectId.ToString()).ToPSADApplication();
        }

        public string GetServicePrincipalsIdByAppId(Guid applicationId)
        {
            return GraphClient.Applications.GetServicePrincipalsIdByAppId(applicationId.ToString())?.Value;
        }

        public IEnumerable<PSADApplication> GetApplicationWithFilters(Rest.Azure.OData.ODataQuery<Application> odataQueryFilter, ulong first = ulong.MaxValue, ulong skip = 0)
        {
            return new GenericPageEnumerable<Application>(
                delegate ()
                {
                    return GraphClient.Applications.List(odataQueryFilter);
                }, GraphClient.Applications.ListNext, first, skip).Select(a => a.ToPSADApplication());
        }

        public PSADServicePrincipal CreateServicePrincipal(CreatePSServicePrincipalParameters createParameters)
        {
            IList<PasswordCredential> passwordCredentials = createParameters.PasswordCredentials != null
                ? createParameters.PasswordCredentials.Select(psCredential => psCredential.ToGraphPasswordCredential()).ToList()
                : null;

            IList<KeyCredential> keyCredentials = createParameters.KeyCredentials != null
                ? createParameters.KeyCredentials.Select(psCredential => psCredential.ToGraphKeyCredential()).ToList()
                : null;

            ServicePrincipalCreateParameters graphParameters = new ServicePrincipalCreateParameters
            {
                AppId = createParameters.ApplicationId.ToString(),
                AccountEnabled = createParameters.AccountEnabled,
                KeyCredentials = keyCredentials,
                PasswordCredentials = passwordCredentials
            };

            try
            {
                return GraphClient.ServicePrincipals.Create(graphParameters).ToPSADServicePrincipal();
            }
            catch (GraphErrorException ce)
            {
                if (ce.Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    User currentUser = GraphClient.SignedInUser.Get();
                    if (currentUser != null && string.Equals(currentUser.UserType, "Guest", StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new InvalidOperationException(ProjectResources.CreateServicePrincipalNotAllowedGuestUser);
                    }
                }

                throw;
            }
        }

        public PSADServicePrincipal RemoveServicePrincipal(Guid objectId)
        {
            var objectIdString = objectId.ToString();
            PSADServicePrincipal servicePrincipal = FilterServicePrincipals(new ADObjectFilterOptions() { Id = objectId.ToString() }).FirstOrDefault();
            if (servicePrincipal != null)
            {
                GraphClient.ServicePrincipals.Delete(objectIdString);
            }
            else
            {
                throw new KeyNotFoundException(string.Format(ProjectResources.ServicePrincipalDoesntExist, objectId));
            }

            return servicePrincipal;
        }
    }
}