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
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.Common.CustomAttributes
{
    [AttributeUsage(
     AttributeTargets.Class,
     AllowMultiple = false)]
    public class CmdletPreviewAttribute : System.Attribute
    {
        public string _message;

        /// <summary>
        ///  EstimatedGaDate assumes value follows "en-US" culture,
        ///  which means dates are written in the month–day–year order like
        ///  July 19, 2023, 19 July 2023, 07/19/2023 or 2023-07-19
        /// </summary>
        public DateTime EstimatedGaDate { get; }

        public bool IsEstimatedGaDateSet { get; } = false;

        public CmdletPreviewAttribute()
        {
            this._message = Resources.PreviewCmdletMessage;
        }

        public CmdletPreviewAttribute(string message)
        {
            this._message = string.IsNullOrEmpty(message) ? Resources.PreviewCmdletMessage : message;
        }

        /// <summary>
        /// Constructor with message and estimated GA date
        /// </summary>
        /// <param name="message">Customized preview message</param>
        /// <param name="estimatedGaDate">EstimatedGaDate assumes value follows "en-US" culture, which means dates are written in the month–day–year order</param>
        public CmdletPreviewAttribute(string message, string estimatedGaDate) : this(message)
        {
            if (DateTime.TryParse(estimatedGaDate, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime result))
            {
                this.EstimatedGaDate = result;
                this.IsEstimatedGaDateSet = true;
            }
        }

        public void PrintCustomAttributeInfo(Action<string> writeOutput)
        {
            writeOutput(this._message);
            if (IsEstimatedGaDateSet)
            {
                writeOutput(string.Format(Resources.PreviewCmdletETAMessage, this.EstimatedGaDate.ToShortDateString()));
            }
        }

        public virtual bool IsApplicableToInvocation(InvocationInfo invocation)
        {
            return true;
        }
    }
}
