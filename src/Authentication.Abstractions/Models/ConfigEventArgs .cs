using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Models
{
    public class ConfigEventArgs : EventArgs, IExtensibleModel
    {
        public string ConfigKey { get; }

        public string ConfigValue { get; }

        public IDictionary<string, string> ExtendedProperties { get; } = new Dictionary<string, string>();

        public ConfigEventArgs(string configKey, string configValue)
        {
            ConfigKey = configKey;
            ConfigValue = configValue;
        }
    }
}
