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

using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="ICmdletContext"/> interface.
    /// </summary>
    public static class CmdletContextExtension
    {
        /// <summary>
        /// Converts the <see cref="ICmdletContext"/> object to a dictionary of extensible parameters.
        /// </summary>
        /// <param name="cmdletContext">The <see cref="ICmdletContext"/> object to convert.</param>
        /// <returns>A dictionary of extensible parameters.</returns>
        public static IDictionary<string, object> ToExtensibleParameters(this ICmdletContext cmdletContext)
        {
            if (cmdletContext != null)
            {
                return new Dictionary<string, object> { { nameof(ICmdletContext), cmdletContext } };
            }
            return null;
        }
    }
}
