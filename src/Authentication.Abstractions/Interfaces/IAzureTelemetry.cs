using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces
{
    /// <summary>
    /// Represents an abstract class for Azure telemetry.
    /// </summary>
    /// <typeparam name="T">The type of telemetry record.</typeparam>
    public abstract class IAzureTelemetry<T>
    {
        private ConcurrentDictionary<string, IList<T>> telemetryDataAccquirer = new ConcurrentDictionary<string, IList<T>>();

        protected int historyKeyCount = 0;
        /// <summary>
        /// Gets the total count of all keys in the telemetry data.
        /// </summary>
        public int KeysAllCount { get => historyKeyCount; }

        /// <summary>
        /// Gets the current count of keys in the telemetry data.
        /// </summary>
        protected int currentKeyCount = 0;
        /// <summary>
        /// Gets the count of empty commandlet contexts in the telemetry data.
        /// </summary>
        public int KeysCurrentCount { get => currentKeyCount; }

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
                if (!telemetryDataAccquirer.ContainsKey(cmdletContext.CmdletId))
                {
                    telemetryDataAccquirer[cmdletContext.CmdletId] = new List<T>();
                    Interlocked.Increment(ref historyKeyCount);
                    Interlocked.Increment(ref currentKeyCount);
                }
                telemetryDataAccquirer[cmdletContext.CmdletId].Add(record);
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
        public IList<T> PopTelemetryRecord(ICmdletContext cmdletContext)
        {
            if (cmdletContext != null && cmdletContext.IsValid)
            {
                if (telemetryDataAccquirer.ContainsKey(cmdletContext.CmdletId))
                {
                    telemetryDataAccquirer.TryRemove(cmdletContext.CmdletId, out IList<T> records);
                    Interlocked.Decrement(ref currentKeyCount);
                    return records;
                }
                else
                {
                    Interlocked.Increment(ref keyNotFoundCount);
                }
            }
            else
            {
                Interlocked.Increment(ref nullCmdletContextCount);
            }
            return null;
        }
    }
}
