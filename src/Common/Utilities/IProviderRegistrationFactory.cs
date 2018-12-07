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

using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using System;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Abstract factory for version-independent provider registration
    /// </summary>
    public interface IProviderRegistrationFactory
    {
        /// <summary>
        /// Get a client that cna register ARM providers
        /// </summary>
        /// <param name="factory">The active AuthenticationFactory</param>
        /// <param name="context">The active context</param>
        /// <param name="messageWriter">A method to log messages</param>
        /// <returns>A registration service for RPs</returns>
        IProviderRegistrar GetRegistrar(IAuthenticationFactory factory, IAzureContext context, Action<string> messageWriter);
    }
}
