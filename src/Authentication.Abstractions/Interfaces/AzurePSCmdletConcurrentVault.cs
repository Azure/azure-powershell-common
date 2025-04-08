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
using System.Linq;
using System.Threading;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces
{
    /// <summary>
    /// A thread-safe implementation of <see cref="IAzurePSCmdletDataVault{T}"/> that stores data records in concurrent queues.
    /// Each queue is associated with a cmdlet context identifier.
    /// </summary>
    /// <typeparam name="T">The type of data records stored in the vault.</typeparam>
    /// <remarks>
    /// This class provides a thread-safe mechanism for storing and retrieving data records
    /// associated with cmdlet contexts. It uses <see cref="ConcurrentDictionary{TKey, TValue}"/> and
    /// <see cref="ConcurrentQueue{T}"/> to ensure thread safety.
    /// </remarks>
    public abstract class AzurePSCmdletConcurrentVault<T> : IAzurePSCmdletDataVault<T>
    {
        private ConcurrentDictionary<string, ConcurrentQueue<T>> dataAccquirer = new ConcurrentDictionary<string, ConcurrentQueue<T>>();
        /// <summary>
        /// Gets the number of unique cmdlet context identifiers currently stored in the vault.
        /// </summary>
        public int KeysCurrentCount { get => dataAccquirer.Keys.Count; }

        protected int nullCmdletContextCount = 0;
        /// <summary>
        /// Gets the count of operations attempted with invalid or null cmdlet contexts.
        /// </summary>
        public int EmptyCmdletContextCount { get => nullCmdletContextCount; }

        protected int keyNotFoundCount = 0;
        /// <summary>
        /// Gets the count of attempts to retrieve data for cmdlet contexts that do not exist in the vault.
        /// </summary>
        public int KeyNotFoundCount { get => keyNotFoundCount; }

        /// <summary>
        /// Adds a data record to the vault for the specified cmdlet context.
        /// </summary>
        /// <param name="cmdletContext">The cmdlet context used as a key for storing the record.</param>
        /// <param name="record">The data record to store.</param>
        /// <returns>
        /// <c>true</c> if the record was successfully added to the vault; otherwise, <c>false</c>.
        /// Returns <c>false</c> if the cmdlet context is null, invalid, or the record is null.
        /// </returns>
        public bool PushDataRecord(ICmdletContext cmdletContext, T record)
        {
            if (cmdletContext != null && cmdletContext.IsValid && record != null)
            {
                var records = dataAccquirer.AddOrUpdate(
                    cmdletContext.CmdletId,
                    k => new ConcurrentQueue<T>(Enumerable.Repeat(record, 1)),
                (key, value) =>
                {
                    value.Enqueue(record);
                    return value;
                });
                return true;
            }
            Interlocked.Increment(ref nullCmdletContextCount);
            return false;
        }

        /// <summary>
        /// Retrieves and removes all data records associated with the specified cmdlet context.
        /// </summary>
        /// <param name="cmdletContext">The cmdlet context used as a key for retrieving records.</param>
        /// <returns>
        /// An enumerable collection of records if the cmdlet context exists in the vault; otherwise, <c>null</c>.
        /// Returns <c>null</c> if the cmdlet context is null or invalid.
        /// </returns>
        public IEnumerable<T> PopDataRecords(ICmdletContext cmdletContext)
        {
            if (cmdletContext != null && cmdletContext.IsValid)
            {
                try
                {
                    if (dataAccquirer.TryRemove(cmdletContext.CmdletId, out var records))
                    {
                        return records;
                    }
                }
                catch (ArgumentNullException)
                {
                }
                Interlocked.Increment(ref keyNotFoundCount);
            }
            else
            {
                Interlocked.Increment(ref nullCmdletContextCount);
            }
            return null;
        }
    }
}
