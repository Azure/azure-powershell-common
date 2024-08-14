using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces
{
    public abstract class IAzureTelemetry <T>
    {
        private ConcurrentDictionary<string, IList<T>> telemetryDataAccquirer = new ConcurrentDictionary<string, IList<T>>();
        
        public bool PushTelemetryRecord(ICmdletContext cmdletContext, T record)
        {
            if (cmdletContext != null && cmdletContext.IsValid && record != null)
            {
                if (!telemetryDataAccquirer.ContainsKey(cmdletContext.CmdletId))
                {
                    telemetryDataAccquirer[cmdletContext.CmdletId] = new List<T>();
                }
                telemetryDataAccquirer[cmdletContext.CmdletId].Add(record);
                return true;
            }
            return false;
        }

        public IList<T> PopTelemetryRecord(ICmdletContext cmdletContext)
        {
            if (cmdletContext != null && cmdletContext.IsValid && telemetryDataAccquirer.ContainsKey(cmdletContext.CmdletId))
            {
                telemetryDataAccquirer.TryRemove(cmdletContext.CmdletId, out IList<T> records);
                return records;
            }
            return null;
        }
    }
}
