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

using Microsoft.WindowsAzure.Commands.Common.Properties;
using System;
using System.Linq;
using System.Text;
using System.Management.Automation;
namespace Microsoft.WindowsAzure.Commands.Common.CustomAttributes
{
    /// <summary>
    /// This attribute is used to mark cmdlets parameters have breaking changes. It provides information about the breaking change, including change description, the version from which the change is deprecated (DeprecateByVersion), the Azure version from which the change is deprecated (DeprecateByAzVersion). This class provides functionality to generate breaking change messages and display information about the breaking changes when needed.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field,
        AllowMultiple = true)]
    public class CmdletParameterBreakingChangeWithVersionAttribute : GenericBreakingChangeWithVersionAttribute
    {
        public string NameOfParameterChanging { get; }

        public string ReplaceMentCmdletParameterName { get; set; } = null;

        public bool IsBecomingMandatory { get; set; } = false;

        public Type OldParamaterType { get; set; }

        public String NewParameterTypeName { get; set; }


        public CmdletParameterBreakingChangeWithVersionAttribute(string nameOfParameterChanging, string deprecateByAzVersion, string deprecateByVersion) :
             base(string.Empty, deprecateByAzVersion, deprecateByVersion)
        {
            this.NameOfParameterChanging = nameOfParameterChanging;
        }

        public CmdletParameterBreakingChangeWithVersionAttribute(string nameOfParameterChanging, string deprecateByAzVersion, string deprecateByVersion, string changeInEffectByDate) :
             base(string.Empty, deprecateByAzVersion, deprecateByVersion, changeInEffectByDate)
        {
            this.NameOfParameterChanging = nameOfParameterChanging;
        }

        protected override string GetAttributeSpecificMessage()
        {
            StringBuilder message = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(ReplaceMentCmdletParameterName))
            {
                if (IsBecomingMandatory)
                {
                    message.Append(string.Format(Resources.BreakingChangeAttributeParameterReplacedMandatory, NameOfParameterChanging, ReplaceMentCmdletParameterName));
                }
                else
                {
                    message.Append(string.Format(Resources.BreakingChangeAttributeParameterReplaced, NameOfParameterChanging, ReplaceMentCmdletParameterName));
                }
            }
            else
            {
                if (IsBecomingMandatory)
                {
                    message.Append(string.Format(Resources.BreakingChangeAttributeParameterMandatoryNow, NameOfParameterChanging));
                }
                else
                {
                    message.Append(string.Format(Resources.BreakingChangeAttributeParameterChanging, NameOfParameterChanging));
                }
            }

            //See if the type of the param is changing
            if (OldParamaterType != null && !string.IsNullOrWhiteSpace(NewParameterTypeName))
            {
                message.Append(string.Format(Resources.BreakingChangeAttributeParameterTypeChange, OldParamaterType.FullName, NewParameterTypeName));
            }
            return message.ToString();
        }

        /// <summary>
        /// See if the bound parameters contain the current parameter, if they do
        /// then the attribute is applicable
        /// If the invocationInfo is null we return true
        /// </summary>
        /// <param name="invocationInfo"></param>
        /// <returns>bool</returns>
        public override bool IsApplicableToInvocation(InvocationInfo invocationInfo)
        {
            bool? applicable = invocationInfo == null ? true : invocationInfo.BoundParameters?.Keys?.Contains(this.NameOfParameterChanging);
            return applicable.HasValue ? applicable.Value : false;
        }
    }
}