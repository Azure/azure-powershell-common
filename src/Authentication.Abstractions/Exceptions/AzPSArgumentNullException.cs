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

using System;
using System.IO;
using System.Runtime.CompilerServices;

using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Exceptions
{
    public class AzPSArgumentException : ArgumentException, IContainsTelemetryErrorData
    {
        public ErrorKind ErrorKind
        {
            get
            {
                return Data.Contains(AzurePSTelemetryKeys.ErrorKindKey) ?
                    Data[AzurePSTelemetryKeys.ErrorKindKey] as ErrorKind : null;
            }

            private set { Data[AzurePSTelemetryKeys.ErrorKindKey] = value; }
        }

        public string DesensitizedErrorMessage
        {
            get
            {
                return Data.Contains(AzurePSTelemetryKeys.DesensitizedErrorMessageKey) ?
                    Data[AzurePSTelemetryKeys.DesensitizedErrorMessageKey]?.ToString() : null;
            }

            private set { Data[AzurePSTelemetryKeys.DesensitizedErrorMessageKey] = value; }
        }

        public int? ErrorLineNumber
        {
            get
            {
                return Data.Contains(AzurePSTelemetryKeys.ErrorLineNumberKey) ?
                    (int?)Data[AzurePSTelemetryKeys.ErrorLineNumberKey] :
                    null;
            }

            private set { Data[AzurePSTelemetryKeys.ErrorLineNumberKey] = value; }
        }

        public string ErrorFileName
        {
            get
            {
                return Data.Contains(AzurePSTelemetryKeys.ErrorFileNameKey) ?
                    Data[AzurePSTelemetryKeys.ErrorFileNameKey]?.ToString() : null;
            }

            private set { Data[AzurePSTelemetryKeys.ErrorFileNameKey] = value; }
        }

        public AzPSArgumentException(
            string message,
            string paramName,
            string desensitizedErrorMessage,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
            : this(message, paramName, desensitizedErrorMessage, ErrorKind.UserError, lineNumber, filePath)
        {
        }

        public AzPSArgumentException(
            string message,
            string paramName,
            string desensitizedErrorMessage,
            ErrorKind errorKind,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
            :base(paramName, message)
        {
            ErrorKind = errorKind;
            if (!string.IsNullOrEmpty(desensitizedErrorMessage))
            {
                DesensitizedErrorMessage = desensitizedErrorMessage;
            }
            ErrorLineNumber = lineNumber;
            if (!string.IsNullOrEmpty(filePath))
            {
                ErrorFileName = Path.GetFileNameWithoutExtension(filePath);
            }
        }
    }
}
