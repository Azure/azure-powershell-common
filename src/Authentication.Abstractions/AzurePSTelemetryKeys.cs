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

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    public class AzurePSTelemetryKeys
    {
        public const string KeyPrefix = "AzPS";

        public static readonly string ErrorKindKey = KeyPrefix + "ErrorKind";
        public static readonly string DesensitizedErrorMessageKey = KeyPrefix + "DesensitizedErrorMessage";
        public static readonly string AuthErrorCodeKey = KeyPrefix + "AzPSAuthErrorCode";
        public static readonly string ParamNameKey = KeyPrefix + "ParamName";
        public static readonly string FileNameKey = KeyPrefix + "FileName";
        public static readonly string MapKeyNameKey = KeyPrefix + "MapKeyName";
        public static readonly string ErrorLineNumberKey = KeyPrefix + "ErrorLineNumber";
        public static readonly string ErrorFileNameKey = KeyPrefix + "ErrorFileName";
    }
}
