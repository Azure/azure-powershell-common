using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.Common.Attributes
{
    /// <summary>
    /// Indicates a cmdlet supports overriding subscription ID via `-SubscriptionId` parameter.
    /// </summary>
    public class SupportsSubscriptionIdAttribute : Attribute
    {
    }
}
