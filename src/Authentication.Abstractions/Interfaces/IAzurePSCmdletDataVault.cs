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

using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces
{
    /// <summary>
    /// Represents a data vault for storing and retrieving cmdlet-specific data records.
    /// </summary>
    /// <typeparam name="T">The type of data records stored in the vault.</typeparam>
    public interface IAzurePSCmdletDataVault<T>
    {
        /// <summary>
        /// Pushes a data record into the vault associated with the specified cmdlet context.
        /// </summary>
        /// <param name="cmdletContext">The cmdlet context to associate with the record.</param>
        /// <param name="record">The data record to store.</param>
        /// <returns>True if the record was successfully pushed; otherwise, false.</returns>
        bool PushDataRecord(ICmdletContext cmdletContext, T record);

        /// <summary>
        /// Retrieves and removes all data records associated with the specified cmdlet context.
        /// </summary>
        /// <param name="cmdletContext">The cmdlet context whose records should be retrieved.</param>
        /// <returns>A collection of data records associated with the cmdlet context.</returns>
        IEnumerable<T> PopDataRecords(ICmdletContext cmdletContext);
    }
}
