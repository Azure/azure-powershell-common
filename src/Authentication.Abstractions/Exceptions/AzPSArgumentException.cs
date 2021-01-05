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

namespace Microsoft.Azure.Commands.Common.Exceptions
{
    public class AzPSArgumentException : ArgumentException, IContainsAzPSErrorData
    {
        public ErrorKind ErrorKind
        {
            get
            {
                return Data.Contains(AzurePSErrorDataKeys.ErrorKindKey) ?
                    Data[AzurePSErrorDataKeys.ErrorKindKey] as ErrorKind : null;
            }

            private set { Data[AzurePSErrorDataKeys.ErrorKindKey] = value; }
        }

        public string DesensitizedErrorMessage
        {
            get
            {
                return Data.Contains(AzurePSErrorDataKeys.DesensitizedErrorMessageKey) ?
                    Data[AzurePSErrorDataKeys.DesensitizedErrorMessageKey]?.ToString() : null;
            }

            private set { Data[AzurePSErrorDataKeys.DesensitizedErrorMessageKey] = value; }
        }

        public int? ErrorLineNumber
        {
            get
            {
                return Data.Contains(AzurePSErrorDataKeys.ErrorLineNumberKey) ?
                    (int?)Data[AzurePSErrorDataKeys.ErrorLineNumberKey] :
                    null;
            }

            private set { Data[AzurePSErrorDataKeys.ErrorLineNumberKey] = value; }
        }

        public string ErrorFileName
        {
            get
            {
                return Data.Contains(AzurePSErrorDataKeys.ErrorFileNameKey) ?
                    Data[AzurePSErrorDataKeys.ErrorFileNameKey]?.ToString() : null;
            }

            private set { Data[AzurePSErrorDataKeys.ErrorFileNameKey] = value; }
        }

        public AzPSArgumentException(
            string message,
            string paramName,
            string desensitizedMessage = null,
            Exception innerException = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
            : this(message, paramName, ErrorKind.UserError, desensitizedMessage, innerException, lineNumber, filePath)
        {
        }

        public AzPSArgumentException(
            string message,
            string paramName,
            ErrorKind errorKind,
            string desensitizedMessage = null,
            Exception innerException = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
            :base(message, paramName)
        {
            ErrorKind = errorKind;
            if (!string.IsNullOrEmpty(desensitizedMessage))
            {
                DesensitizedErrorMessage = desensitizedMessage;
            }
            ErrorLineNumber = lineNumber;
            if (!string.IsNullOrEmpty(filePath))
            {
                ErrorFileName = Path.GetFileNameWithoutExtension(filePath);
            }
        }
    }
}
