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

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Contains configuration for how PowerShell renders text.
    /// A subset of https://github.com/PowerShell/PowerShell/blob/f0076b9d883aa0ed07fb2a5c2be5f38f5d945e1e/src/System.Management.Automation/FormatAndOutput/common/PSStyle.cs#L173
    /// </summary>
    public sealed class PSStyle
    {
        /// <summary>
        /// Contains background colors.
        /// </summary>
        public sealed class BackgroundColor
        {
            /// <summary>
            /// Gets the color blue.
            /// </summary>
            public static string Blue { get; } = "\x1b[44m";
        }

        public static string Reset { get; } = "\x1b[0m";

        /// <summary>
        /// Gets value to turn off underlined.
        /// </summary>
        public static string UnderlineOff { get; } = "\x1b[24m";

        /// <summary>
        /// Gets value to turn on underlined.
        /// </summary>
        public static string Underline { get; } = "\x1b[4m";

        /// <summary>
        /// Gets value to turn off bold.
        /// </summary>
        public static string BoldOff { get; } = "\x1b[22m";

        /// <summary>
        /// Gets value to turn on bold.
        /// </summary>
        public static string Bold { get; } = "\x1b[1m";
    }
}
