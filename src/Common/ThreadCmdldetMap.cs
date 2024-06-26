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
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Microsoft.Azure.Commands.Common.Authentication.Utilities
{
    public class ThreadCmdldetMap
    {
        private ConcurrentDictionary<int, string> map = new ConcurrentDictionary<int, string>();

        private static string logFile = Path.Combine(AzureSession.Instance.ProfileDirectory, "threadCmdletMap.log");

        private void WriteLog(string log)
        {
            if (!File.Exists(logFile))
            {
                using (StreamWriter sw = File.CreateText(logFile))
                {
                    sw.WriteLine(log);
                }
            }
            using (StreamWriter sw = File.AppendText(logFile))
            {
                sw.WriteLine(log);
            }
        }

        public void PushCmdletId(string cmdletId)
        {
#if DEBUG
            WriteLog($"[PushCmdletId] ThreadId={Thread.CurrentThread.ManagedThreadId}, CmdletId={cmdletId}");
#endif
            map[Thread.CurrentThread.ManagedThreadId] = cmdletId;
        }

        public string PopCmdletId()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            if (map.ContainsKey(id))
            {
                map.TryRemove(id, out string cmdletId);
#if DEBUG
                WriteLog($"[PopCmdletId] ThreadId={id}, CmdletId={cmdletId}");
#endif
                return cmdletId;
            }
#if DEBUG
            WriteLog($"[PopCmdletId] ThreadId={id}, CmdletId=NotFound");
#endif
            return string.Empty;
        }
    }
}
