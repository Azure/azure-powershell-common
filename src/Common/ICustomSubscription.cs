using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.Common
{
    // todo: rename
    public interface ICustomSubscription
    {
        Guid SubscriptionId { get; set; }
    }
}
