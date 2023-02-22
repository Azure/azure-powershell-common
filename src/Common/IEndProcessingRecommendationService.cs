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

using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    /// <summary>
    /// A service to give recommendations based on the input of the user.
    /// For example we could recommend a region which costs less.
    /// </summary>
    public abstract class IEndProcessingRecommendationService
    {
        /// <summary>
        /// Process the cmdlet execution and display recommendation messages if any.
        /// </summary>
        /// <param name="azurePSCmdlet">Cmdlet instance, for writing messages.</param>
        /// <param name="myInvocation">Contains info about cmdlet, module, parameters.</param>
        /// <param name="_qosEvent">To record successful recommendation.</param>
        public abstract void Process(AzurePSCmdlet azurePSCmdlet, InvocationInfo myInvocation, AzurePSQoSEvent _qosEvent);
    }
}
