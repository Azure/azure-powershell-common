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

namespace Microsoft.WindowsAzure.Commands.Common.CustomAttributes
{
    /// <summary>
    /// This attribute is used to mark cmdlets as deprecated. It provides information about the breaking change, including change description, the version from which the change is deprecated (DeprecateByVersion), the Azure version from which the change is deprecated (DeprecateByAzVersion). This class provides functionality to generate breaking change messages and display information about the breaking changes when needed.
    /// </summary>
    [AttributeUsage(
     AttributeTargets.Class,
     AllowMultiple = true)]
    public class CmdletDeprecationWithVersionAttribute : GenericBreakingChangeWithVersionAttribute
    {
        public string ReplacementCmdletName { get; set; }

        public CmdletDeprecationWithVersionAttribute(string deprecateByAzVersion, string deprecateByVersion) :
             base(string.Empty, deprecateByAzVersion, deprecateByVersion)
        {
        }

        public CmdletDeprecationWithVersionAttribute(string deprecateByAzVersion, string deprecateByVersion, string changeInEffectByDate) :
             base(string.Empty, deprecateByAzVersion, deprecateByVersion, changeInEffectByDate)
        {
        }

        protected override string GetAttributeSpecificMessage()
        {
            if (string.IsNullOrWhiteSpace(ReplacementCmdletName))
            {
                return Resources.BreakingChangesAttributesCmdLetDeprecationMessageNoReplacement;
            }
            else
            {
                return string.Format(Resources.BreakingChangesAttributesCmdLetDeprecationMessageWithReplacement, ReplacementCmdletName);
            }
        }
    }
}