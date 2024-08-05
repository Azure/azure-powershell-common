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

using System.Management.Automation.Host;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Contains configuration for how PowerShell renders text.
    /// A subset of https://github.com/PowerShell/PowerShell/blob/f0076b9d883aa0ed07fb2a5c2be5f38f5d945e1e/src/System.Management.Automation/FormatAndOutput/common/PSStyle.cs#L173
    /// </summary>
    public sealed class PSStyle
    {
        private static bool? _isEscapeSequenceSupported = null;

        /// <summary>
        /// Initializes default settings of PSStyle according to the environment.
        /// Note: This function should be invoked only in Az.Accounts.
        /// </summary>
        /// <param name="host">PowerShell Console Host</param>
        public static void Initialize(PSHost host)
        {
            _isEscapeSequenceSupported = host?.UI?.SupportsVirtualTerminal;
        }

        /// <summary>
        /// Contains background colors.
        /// </summary>
        public sealed class BackgroundColor
        {
            /// <summary>
            /// Gets the color black.
            /// </summary>
            public static string Black
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[40m" : "";
                }
            }

            /// <summary>
            /// Gets the color red.
            /// </summary>
            public static string Red
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[41m" : "";
                }
            }

            /// <summary>
            /// Gets the color green.
            /// </summary>
            public static string Green
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[42m" : "";
                }
            }

            /// <summary>
            /// Gets the color yellow.
            /// </summary>
            public static string Yellow
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[43m" : "";
                }
            }

            /// <summary>
            /// Gets the color blue.
            /// </summary>
            public static string Blue
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[44m" : "";
                }
            }

            /// <summary>
            /// Gets the color magenta.
            /// </summary>
            public static string Magenta
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[45m" : "";
                }
            }

            /// <summary>
            /// Gets the color cyan.
            /// </summary>
            public static string Cyan
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[46m" : "";
                }
            }

            /// <summary>
            /// Gets the color white.
            /// </summary>
            public static string White
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[47m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright black.
            /// </summary>
            public static string BrightBlack
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[100m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright red.
            /// </summary>
            public static string BrightRed
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[101m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright green.
            /// </summary>
            public static string BrightGreen
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[102m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright yellow.
            /// </summary>
            public static string BrightYellow
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[103m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright blue.
            /// </summary>
            public static string BrightBlue
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[104m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright magenta.
            /// </summary>
            public static string BrightMagenta
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[105m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright cyan.
            /// </summary>
            public static string BrightCyan
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[106m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright white.
            /// </summary>
            public static string BrightWhite
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[107m" : "";
                }
            }
        }

        /// <summary>
        /// Contains foreground colors.
        /// </summary>
        public sealed class ForegroundColor
        {

            /// <summary>
            /// Gets the color black.
            /// </summary>
            public static string Black
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[30m" : "";
                }
            }

            /// <summary>
            /// Gets the color red.
            /// </summary>
            public static string Red
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[31m" : "";
                }
            }

            /// <summary>
            /// Gets the color green.
            /// </summary>
            public static string Green
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[32m" : "";
                }
            }

            /// <summary>
            /// Gets the color yellow.
            /// </summary>
            public static string Yellow
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[33m" : "";
                }
            }

            /// <summary>
            /// Gets the color blue.
            /// </summary>
            public static string Blue
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[34m" : "";
                }
            }

            /// <summary>
            /// Gets the color magenta.
            /// </summary>
            public static string Magenta
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[35m" : "";
                }
            }

            /// <summary>
            /// Gets the color cyan.
            /// </summary>
            public static string Cyan
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[36m" : "";
                }
            }

            /// <summary>
            /// Gets the color white.
            /// </summary>
            public static string White
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[37m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright black.
            /// </summary>
            public static string BrightBlack
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[90m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright red.
            /// </summary>
            public static string BrightRed
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[91m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright green.
            /// </summary>
            public static string BrightGreen
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[92m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright yellow.
            /// </summary>
            public static string BrightYellow
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[93m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright blue.
            /// </summary>
            public static string BrightBlue
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[94m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright magenta.
            /// </summary>
            public static string BrightMagenta
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[95m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright cyan.
            /// </summary>
            public static string BrightCyan
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[96m" : "";
                }
            }

            /// <summary>
            /// Gets the color bright white.
            /// </summary>
            public static string BrightWhite
            {
                get
                {
                    return _isEscapeSequenceSupported == true ? "\x1b[97m" : "";
                }
            }
        }

        public static string Reset
        {
            get
            {
                return _isEscapeSequenceSupported == true ? "\x1b[0m" : "";
            }
        }

        /// <summary>
        /// Gets value to turn off underlined.
        /// </summary>
        public static string UnderlineOff
        {
            get
            {
                return _isEscapeSequenceSupported == true ? "\x1b[24m" : "";
            }
        }

        /// <summary>
        /// Gets value to turn on underlined.
        /// </summary>
        public static string Underline
        {
            get
            {
                return _isEscapeSequenceSupported == true ? "\x1b[4m" : "";
            }
        }

        /// <summary>
        /// Gets value to turn off bold.
        /// </summary>
        public static string BoldOff
        {
            get
            {
                return _isEscapeSequenceSupported == true ? "\x1b[22m" : "";
            }
        }

        /// <summary>
        /// Gets value to turn on bold.
        /// </summary>
        public static string Bold
        {
            get
            {
                return _isEscapeSequenceSupported == true ? "\x1b[1m" : "";
            }
        }
    }
}
