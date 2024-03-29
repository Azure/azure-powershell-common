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

using Microsoft.WindowsAzure.Commands.Common.Properties;
using System;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.Common.CustomAttributes
{
    /// <summary>
    /// This attribute is used to mark cmdlets output type has breaking changes. It provides information about the breaking change, including change description, the version from which the change is deprecated (DeprecateByVersion), the Azure version from which the change is deprecated (DeprecateByAzVersion). This class provides functionality to generate breaking change messages and display information about the breaking changes when needed.
    /// </summary>
    [AttributeUsage(
     AttributeTargets.Class,
     AllowMultiple = true)]
    public class CmdletOutputBreakingChangeWithVersionAttribute : GenericBreakingChangeWithVersionAttribute
    {
        public Type DeprecatedCmdLetOutputType { get; }

        //This is still a String instead of a Type as this 
        //might be undefined at the time of adding the attribute
        public string ReplacementCmdletOutputTypeName { get; set; }

        public string[] DeprecatedOutputProperties { get; set; }

        public string[] NewOutputProperties { get; set; }


        public CmdletOutputBreakingChangeWithVersionAttribute(Type deprecatedCmdletOutputTypeName, string deprecateByAzVersion, string deprecateByVersion) :
             base(string.Empty, deprecateByAzVersion, deprecateByVersion)
        {
            this.DeprecatedCmdLetOutputType = deprecatedCmdletOutputTypeName;
        }

        public CmdletOutputBreakingChangeWithVersionAttribute(Type deprecatedCmdletOutputTypeName, string deprecateByAzVersion, string deprecateByVersion, string changeInEfectByDate) :
             base(string.Empty, deprecateByAzVersion, deprecateByVersion, changeInEfectByDate)
        {
            this.DeprecatedCmdLetOutputType = deprecatedCmdletOutputTypeName;
        }

        protected override string GetAttributeSpecificMessage()
        {
            StringBuilder message = new StringBuilder();

            //check for the deprecation scenario
            if (string.IsNullOrWhiteSpace(ReplacementCmdletOutputTypeName) && NewOutputProperties == null && DeprecatedOutputProperties == null && string.IsNullOrWhiteSpace(ChangeDescription))
            {
                message.Append(string.Format(Resources.BreakingChangesAttributesCmdLetOutputTypeDeprecated, DeprecatedCmdLetOutputType.FullName));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(ReplacementCmdletOutputTypeName))
                {
                    message.Append(string.Format(Resources.BreakingChangesAttributesCmdLetOutputChange1, DeprecatedCmdLetOutputType.FullName, ReplacementCmdletOutputTypeName));
                }
                else
                {
                    message.Append(string.Format(Resources.BreakingChangesAttributesCmdLetOutputChange2, DeprecatedCmdLetOutputType.FullName));
                }

                if (DeprecatedOutputProperties != null && DeprecatedOutputProperties.Length > 0)
                {
                    message.Append(Resources.BreakingChangesAttributesCmdLetOutputPropertiesRemoved);
                    foreach (string property in DeprecatedOutputProperties)
                    {
                        message.Append(" '" + property + "'");
                    }
                }

                if (NewOutputProperties != null && NewOutputProperties.Length > 0)
                {
                    message.Append(Resources.BreakingChangesAttributesCmdLetOutputPropertiesAdded);
                    foreach (string property in NewOutputProperties)
                    {
                        message.Append(" '" + property + "'");
                    }
                }
            }
            return message.ToString();
        }
    }
}