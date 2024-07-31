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

namespace Microsoft.Azure.PowerShell.Common.Config
{
    /// <summary>
    /// Options for updating a config. Used as input of <see cref="IConfigManager.UpdateConfig(UpdateConfigOptions)"/>.
    /// </summary>
    public class UpdateConfigOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateConfigOptions"/> class with the specified key, value, and scope.
        /// </summary>
        /// <param name="key">The key of the config.</param>
        /// <param name="value">The value of the config.</param>
        /// <param name="scope">The scope of the config.</param>
        public UpdateConfigOptions(string key, object value, ConfigScope scope)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Scope = scope;
            Value = value;
        }

        /// <summary>
        /// Gets the key of the config.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the value of the config.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Gets or sets the scope of the config.
        /// </summary>
        public ConfigScope Scope { get; set; }

        /// <summary>
        /// Gets or sets the module or cmdlet that the config applies to.
        /// If null, it applies to all.
        /// </summary>
        public string AppliesTo { get; set; } = null;
    }
}
