using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.Common
{
    interface ICustomSubscription
    {
        string SubscriptionId { get; set; }
    }
}
