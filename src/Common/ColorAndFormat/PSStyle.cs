namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Contains configuration for how PowerShell renders text.
    /// A subset of https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/FormatAndOutput/common/PSStyle.cs
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
