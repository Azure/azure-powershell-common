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

namespace Microsoft.Azure.PowerShell.Common.Config
{
    /// <summary>
    /// This class stores keys of pre-defined configs.
    /// </summary>
    /// <remarks>
    /// All keys should be defined in ConfigKeys class in Azure/azure-powershell repo.
    /// If the key is used in common code, duplicate it here.
    /// Keys defined here should NEVER be removed or changed to prevent breaking change.
    /// </remarks>
    public static class ConfigKeysForCommon
    {
        public const string EnableInterceptSurvey = "EnableInterceptSurvey";
        public const string DisplayBreakingChangeWarning = "DisplayBreakingChangeWarning";
        public const string EnableDataCollection = "EnableDataCollection";
    }
}
