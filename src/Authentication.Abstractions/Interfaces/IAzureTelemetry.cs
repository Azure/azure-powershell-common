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
    /// Represents an abstract class for Azure telemetry.
    /// </summary>
    /// <typeparam name="T">The type of telemetry record.</typeparam>
    public abstract class IAzureTelemetry<T>
    {
        private ConcurrentDictionary<string, ConcurrentQueue<T>> telemetryDataAccquirer = new ConcurrentDictionary<string, ConcurrentQueue<T>>();

        /// <summary>
        /// Gets the current count of keys in the telemetry data.
        /// </summary>
        protected int currentKeyCount = 0;
        /// <summary>
        /// Gets the count of empty commandlet contexts in the telemetry data.
        /// </summary>
        public int KeysCurrentCount { get => telemetryDataAccquirer.Keys.Count; }

        protected int nullCmdletContextCount = 0;
        public int EmptyCmdletContextCount { get => nullCmdletContextCount; }

        protected int keyNotFoundCount = 0;
        /// <summary>
        /// Gets the count of key not found occurrences in the telemetry data.
        /// </summary>
        public int KeyNotFoundCount { get => keyNotFoundCount; }

        /// <summary>
        /// Pushes a telemetry record to the telemetry data.
        /// </summary>
        /// <param name="cmdletContext">The commandlet context.</param>
        /// <param name="record">The telemetry record.</param>
        /// <returns><c>true</c> if the telemetry record was successfully pushed; otherwise, <c>false</c>.</returns>
        public bool PushTelemetryRecord(ICmdletContext cmdletContext, T record)
        {
            if (cmdletContext != null && cmdletContext.IsValid && record != null)
            {
                var records = telemetryDataAccquirer.AddOrUpdate(
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
        /// Pops a telemetry record from the telemetry data.
        /// </summary>
        /// <param name="cmdletContext">The commandlet context.</param>
        /// <returns>The telemetry records associated with the commandlet context, or <c>null</c> if not found.</returns>
        public IEnumerable<T> PopTelemetryRecord(ICmdletContext cmdletContext)
        {
            if (cmdletContext != null && cmdletContext.IsValid)
            {
                try
                {
                    if (telemetryDataAccquirer.TryRemove(cmdletContext.CmdletId, out var records))
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
