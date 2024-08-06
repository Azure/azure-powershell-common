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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Models
{
    /// <summary>
    /// Recorded value for config in telemetry
    /// </summary>
    public class ConfigMetrics: IExtensibleModel
    {
        /// <summary>
        /// The unique key of config. It's required for recording config telemetry.
        /// </summary>
        public string ConfigKey { get; private set; }

        /// <summary>
        /// The unique telemetry key of config. It's required for recording config telemetry. If not provided, it's same with ConfigKey.
        /// </summary>
        public string TelemetryKey { get; private set; }

        /// <summary>
        /// Config value in string format. It's required for recording config telemetry.
        /// </summary>
        public string ConfigValue { get; private set; }

        public IDictionary<string, string> ExtendedProperties { get; } = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ConfigMetrics(string configKey, string configValue): this(configKey, configKey, configValue){}

        public ConfigMetrics(string configKey, string telemetryKey, string configValue)
        {
            this.ConfigKey = configKey;
            this.TelemetryKey = telemetryKey;
            this.ConfigValue = configValue;
        }
    }
}
