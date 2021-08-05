using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.Common.Attributes
{
    /// <summary>
    /// Indicates a cmdlet supports overriding subscription ID via -SubscriptionId parameter.
    /// </summary>
    public class SupportsSubscriptionIdAttribute : Attribute
    {
    }

    public static class SupportsSubscriptionIdAttributeExtensions
    {
        // todo: maybe move this to a common place
        // todo: maybe: cmdlet.HasAttribute(typeof(xxxAttribute))
        public static bool HasSupportsSubscriptionIdAttribute(this PSCmdlet cmdlet)
        {
            return Attribute.GetCustomAttribute(cmdlet.GetType(), typeof(SupportsSubscriptionIdAttribute)) != null;
        }
    }
}
