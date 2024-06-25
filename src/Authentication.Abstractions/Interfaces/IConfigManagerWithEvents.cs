using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Models;
using Microsoft.Azure.PowerShell.Common.Config;

using System;

namespace Microsoft.Azure.Commands.Common.Authentication.Config
{
    public interface IConfigManagerWithEvents: IConfigManager
    {
        event EventHandler<ConfigEventArgs> ConfigRead;
        event EventHandler<ConfigEventArgs> ConfigUpdated;
        event EventHandler<ConfigEventArgs> ConfigCleared;
    }
}
