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
using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// Represents the context for an Azure cmdlet.
    /// </summary>
    public class AzureCmdletContext : ICmdletContext
    {
        private string cmdletId;

        /// <summary>
        /// Represents a constant value indicating that no cmdlet context is specified.
        /// </summary>
        public const ICmdletContext CmdletNone = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCmdletContext"/> class with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the cmdlet.</param>
        public AzureCmdletContext(string id)
        {
            cmdletId = id;
        }

        /// <summary>
        /// Gets or sets the ID of the cmdlet.
        /// </summary>
        public string CmdletId
        {
            get => cmdletId;
            set => cmdletId = value;
        }

        /// <summary>
        /// Gets a value indicating whether the cmdlet context is valid.
        /// </summary>
        public bool IsValid
        {
            get => !string.IsNullOrEmpty(cmdletId);
        }
    }
}
