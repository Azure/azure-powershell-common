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

using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.Common.Utilities
{
    /// <summary>
    /// String formatter for bound parameters for telemetry purpose.
    /// </summary>
    public interface IParameterTelemetryFormatter
    {
        /// <summary>
        /// Format the bound parameters to a string without sensitive data.
        /// </summary>
        /// <param name="invocation">Info about cmdlet invocation</param>
        /// <returns>The formatted string.</returns>
        string FormatParameters(InvocationInfo invocation);
    }
}
