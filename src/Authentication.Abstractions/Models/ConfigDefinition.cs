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
using System.Collections.Generic;

namespace Microsoft.Azure.PowerShell.Common.Config
{
    /// <summary>
    /// Represents the definition of a config of Azure PowerShell.
    /// </summary>
    public abstract class ConfigDefinition
    {
        /// <summary>
        /// Gets the default value of this config.
        /// </summary>
        public abstract object DefaultValue { get; }

        /// <summary>
        /// Gets the unique key of this config.
        /// </summary>
        /// <remarks>It is also used as the name of the PowerShell parameter which maps to this config, so the key must follow the design guideline and conventions. See <see href="https://github.com/Azure/azure-powershell/blob/main/documentation/development-docs/design-guidelines/parameter-best-practices.md#parameter-best-practices">Parameter Best Practices</see>.</remarks>
        public abstract string Key { get; }

        /// <summary>
        /// Gets the help message or description of the config.
        /// It is also used as the help message of the PowerShell parameter which maps to this config.
        /// </summary>
        public abstract string HelpMessage { get; }

        /// <summary>
        /// Gets the name of the environment variable that can control this config.
        /// </summary>
        /// <remarks>
        /// For most cases, where the config is connected to only 1 environment variable,
        /// and the value can be parsed without extra logic, use this property,
        /// else override <see cref="ParseFromEnvironmentVariables(IReadOnlyDictionary{string, string})"/>.
        /// </remarks>
        protected virtual string EnvironmentVariableName { get; } = null;

        /// <summary>
        /// Customizes how to parse config value from environment variables.
        /// </summary>
        /// <param name="environmentVariables">All the environment variables.</param>
        /// <returns>The parsed config value, in string. Returns null if the environment variable of this config is not set.</returns>
        /// <remarks>
        /// Note: use <see cref="EnvironmentVariableName"/> if there's no need for customized parsing logic. <br />
        /// The return type is string because it will be further parsed into other types (int, bool...) by the config provider.
        /// Make sure the return value matches <see cref="ValueType"/>.
        /// For example if <see cref="ValueType"/> is bool, the return value cannot be like "Disabled".
        /// </remarks>
        public virtual string ParseFromEnvironmentVariables(IReadOnlyDictionary<string, string> environmentVariables)
        {
            if (string.IsNullOrEmpty(EnvironmentVariableName))
            {
                return null;
            }
            else
            {
                return environmentVariables.TryGetValue(EnvironmentVariableName, out string value) ? value : null;
            }
        }

        /// <summary>
        /// Gets how the config can be applied to.
        /// </summary>
        public virtual IReadOnlyCollection<AppliesTo> CanApplyTo => new AppliesTo[] { AppliesTo.Az, AppliesTo.Module, AppliesTo.Cmdlet };

        /// <summary>
        /// Gets the type of the value of this config.
        /// </summary>
        public abstract Type ValueType { get; }

        /// <summary>
        /// Override in derived classes to validate the input value. Throws an exception if not.
        /// </summary>
        /// <param name="value">The value to check.</param>
        public virtual void Validate(object value) { }

        /// <summary>
        /// Override in derived classes to perform side effects of applying the config value.
        /// If a exception is thrown, the config will not be updated.
        /// </summary>
        /// <param name="value">Value of the config to apply.</param>
        public virtual void Apply(object value) { }
    }
}
