using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces
{
    public abstract class IAzureTelemetry <T>
    {
        private ConcurrentDictionary<string, IList<T>> telemetryDataAccquirer = new ConcurrentDictionary<string, IList<T>>();

        protected int historyKeyCount = 0;
        public int KeysAllCount { get => historyKeyCount; }

        protected int currentKeyCount = 0;
        public int KeysCurrentCount { get => currentKeyCount; }

        protected int nullCmdletContextCount = 0;
        public int EmptyCmdletContextCount { get => nullCmdletContextCount; }

        protected int keyNotFoundCount = 0;
        public int KeyNotFoundCount { get => keyNotFoundCount; }

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
