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
    [AttributeUsage(
     AttributeTargets.Class,
     AllowMultiple = true)]
    public class CmdletDeprecationWithVersionAttribute : GenericBreakingChangeAttribute
    {
        public string ReplacementCmdletName { get; set; }

        public CmdletDeprecationWithVersionAttribute(string deprecateByVersion) :
             base(string.Empty, deprecateByVersion)
        {
        }

        public CmdletDeprecationWithVersionAttribute(string deprecateByVersion, string changeInEffectByDate) :
             base(string.Empty, deprecateByVersion, changeInEffectByDate)
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
