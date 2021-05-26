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

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.PowerShell.Share.Survey
{
    public class SurveyHelper
    {
        private const int CountExpiredDays = 30;
        private const int LockExpiredDays = 30;
        private const int SurveyTriggerCount = 3;
        private const int FlushFrequecy = 5;
        private const int DelayForSecondPrompt = 2;
        private const int DelayForThirdPrompt = 5;

        private static SurveyHelper Instance;

        private int FlushCount;

        private static string SurveySchedulePath = AzurePowerShell.SurveyScheduleInfoFile;

        //InterceptTriggered could be incorrect because shared by different threads
        private int InterceptTriggered;

        private DateTime LastPromptDate { get; set; }

        private ConcurrentDictionary<string, ModuleInfo> InternalMap { get; set; }

        private bool IgnoreSchedule;

        private bool IsDisabledFromEnv => "Disabled".Equals(Environment.GetEnvironmentVariable(AzurePowerShell.AzurePSInterceptSurvey), StringComparison.OrdinalIgnoreCase);

        private readonly string CurrentDate;

        private readonly DateTime Today;

        private SurveyHelper()
        {
            CurrentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            Today = Convert.ToDateTime(CurrentDate);
            IgnoreSchedule = false;
            LastPromptDate = DateTime.MinValue;
            InternalMap = new ConcurrentDictionary<string, ModuleInfo>();
            Interlocked.Exchange(ref FlushCount, 0);
            InterceptTriggered = 0;
        }

        public static SurveyHelper GetInstance()
        {
            if (Instance == null)
            {
                Instance = new SurveyHelper();
            }
            return Instance;
        }

        public bool ShouldPropmtSurvey(string moduleName, Version moduleVersion)
        {
            if (IgnoreSchedule || IsDisabledFromEnv)
            {
                return false;
            }

            UpdatedScheduleInfo updatedInfo = null;
            int majorVersion = moduleVersion.Major;

            if (InternalMap.Count == 0)
            {
                ReadFromStream(null, out updatedInfo);
            }

            if (!InternalMap.ContainsKey(moduleName))
            {
                InternalMap[moduleName] = new ModuleInfo() {
                    Name = moduleName,
                    MajorVersion = majorVersion,
                    ActiveDays = 1,
                    FirstActiveDate = CurrentDate,
                    LastActiveDate = CurrentDate,
                    Enabled = true
                };
                if (ReadFromStream(null, out updatedInfo) && updatedInfo.ShouldWrite)
                {
                    TryFlushAsync(updatedInfo.Info);
                }
                return false;
            }

            ModuleInfo cur = InternalMap[moduleName];
            //LastPromptDate.CompareTo(DateTime.MinValue) > 0 means survey is locked, otherwise lock free
            if (LastPromptDate > DateTime.MinValue && Today > LastPromptDate.AddDays(LockExpiredDays))
            {
                LastPromptDate = DateTime.MinValue;
                InterceptTriggered = 0;
            }

            //if version is not current version and not deprecated, start count for this version
            if (majorVersion > cur.MajorVersion)
            {
                cur.MajorVersion = majorVersion;
                cur.FirstActiveDate = CurrentDate;
                cur.LastActiveDate = CurrentDate;
                cur.ActiveDays = 1;
                if (ReadFromStream(null, out updatedInfo) && updatedInfo.ShouldWrite)
                {
                    TryFlushAsync(updatedInfo.Info);
                }
            }
            else if (majorVersion == cur.MajorVersion)
            {
                //prompt surey
                if (cur.ActiveDays == SurveyTriggerCount && InterceptTriggered == 0 && LastPromptDate == DateTime.MinValue
                 || cur.ActiveDays == SurveyTriggerCount + 1 && InterceptTriggered == 1 && LastPromptDate == Convert.ToDateTime(cur.LastActiveDate) && Today == LastPromptDate.AddDays(DelayForSecondPrompt)
                 || cur.ActiveDays == SurveyTriggerCount + 2 && InterceptTriggered == 2 && LastPromptDate == Convert.ToDateTime(cur.LastActiveDate) && Today == LastPromptDate.AddDays(DelayForThirdPrompt))
                {
                    LastPromptDate = Today;                   
                    cur.LastActiveDate = CurrentDate;
                    cur.ActiveDays = cur.ActiveDays + 1;
                    InterceptTriggered = InterceptTriggered == 2 ? 0 : InterceptTriggered + 1;                  
                    if (ReadFromStream(moduleName, out updatedInfo) && updatedInfo.ShouldWrite)
                    {
                        TryFlushAsync(updatedInfo.Info, updatedInfo.ShouldPrompt);
                        return updatedInfo.ShouldPrompt;
                    }             
                }
                else if (cur.ActiveDays == SurveyTriggerCount + 1 && InterceptTriggered == 1 && LastPromptDate == Convert.ToDateTime(cur.LastActiveDate) && Today > LastPromptDate.AddDays(DelayForSecondPrompt)
                      || cur.ActiveDays == SurveyTriggerCount + 2 && InterceptTriggered == 2 && LastPromptDate == Convert.ToDateTime(cur.LastActiveDate) && Today > LastPromptDate.AddDays(DelayForThirdPrompt))
                {
                    InterceptTriggered = 0;
                    cur.ActiveDays = 0;
                    if (ReadFromStream(null, out updatedInfo) && updatedInfo.ShouldWrite)
                    {
                        TryFlushAsync(updatedInfo.Info);
                    }
                }
                else if (cur.ActiveDays < SurveyTriggerCount)
                {
                    //date is later than last active date and not expired
                    if (Today > Convert.ToDateTime(cur.LastActiveDate))
                    {
                        if (Today <= Convert.ToDateTime(cur.FirstActiveDate).AddDays(CountExpiredDays))
                        {
                            cur.ActiveDays += 1;
                            cur.LastActiveDate = CurrentDate;
                        }
                        else
                        {
                            cur.FirstActiveDate = CurrentDate;
                            cur.LastActiveDate = CurrentDate;
                            cur.ActiveDays = 1;
                        }
                        if (ReadFromStream(null, out updatedInfo) && updatedInfo.ShouldWrite)
                        {
                            TryFlushAsync(updatedInfo.Info);
                        }
                    }
                }
            }
            return false;
        }

        private UpdatedScheduleInfo MergeScheduleInfo(ScheduleInfo externalScheduleInfo, string moduleName)
        {
            bool ShouldWrite = false;
            bool ShouldPrompt = false;
            DateTime externalLastPromptDate = Convert.ToDateTime(externalScheduleInfo?.LastPromptDate);
            ScheduleInfo tmp = new ScheduleInfo() { Modules = new List<ModuleInfo>() };
            IDictionary<string, ModuleInfo> externalMap = new Dictionary<string, ModuleInfo>();
            externalScheduleInfo?.Modules?.ForEach<ModuleInfo>(x => externalMap[x.Name] = x);

            HashSet<string> moduleNames = new HashSet<string>(InternalMap.Keys);
            moduleNames.UnionWith(new HashSet<string>(externalMap.Keys));

            moduleNames.ForEach<string>(name =>
            {
                ModuleInfo item = null;
                if (InternalMap.ContainsKey(name) && (!externalMap.ContainsKey(name) || Convert.ToDateTime(InternalMap[name].LastActiveDate) > Convert.ToDateTime(externalMap[name].LastActiveDate)))
                {
                    item = new ModuleInfo(InternalMap[name]);
                    ShouldWrite = true;
                    ShouldPrompt = name.Equals(moduleName);
                }
                else
                {
                    if (externalLastPromptDate != DateTime.MinValue && Convert.ToDateTime(externalMap[name].LastActiveDate) == externalLastPromptDate)
                    {
                        LastPromptDate = externalLastPromptDate;
                        if (externalScheduleInfo.InterceptTriggered == 1 && externalMap[name].ActiveDays == SurveyTriggerCount + 1)
                        {
                            InterceptTriggered = 1;
                        }
                        else if (externalScheduleInfo.InterceptTriggered == 2 && externalMap[name].ActiveDays == SurveyTriggerCount + 2)
                        {
                            InterceptTriggered = 2;
                        }
                        else if (externalScheduleInfo.InterceptTriggered ==0 && externalMap[name].ActiveDays == SurveyTriggerCount + 3)
                        {
                            InterceptTriggered = 0;
                        }
                    }
                    item = new ModuleInfo(externalMap[name]);
                    InternalMap[name] = item;
                }
                tmp.Modules.Add(item);
            });
            tmp.InterceptTriggered = InterceptTriggered;
            tmp.LastPromptDate = LastPromptDate.ToString("yyyy-MM-dd");

            return new UpdatedScheduleInfo() { Info = tmp, ShouldWrite = ShouldWrite, ShouldPrompt = ShouldPrompt};
        }

        private bool ReadFromStream(string moduleName, out UpdatedScheduleInfo info)
        {
            StreamReader sr = null;
            info = new UpdatedScheduleInfo() { Info = null, ShouldPrompt = false, ShouldWrite = false};
            try
            {
                if (File.Exists(SurveySchedulePath))
                {
                    sr = new StreamReader(new FileStream(SurveySchedulePath, FileMode.Open, FileAccess.Read, FileShare.None));
                    int size = (int)sr.BaseStream.Length;
                    char[] buffer = new char[size];
                    sr.ReadBlock(buffer, 0, size);
                    ScheduleInfo tmp = JsonConvert.DeserializeObject<ScheduleInfo>(new string(buffer));
                    info = MergeScheduleInfo(tmp, moduleName);
                }
                else
                {
                    info = MergeScheduleInfo(null, moduleName);
                }   
            }
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException)
                {
                    this.IgnoreSchedule = true;
                }
                //deserialize failed, means content of file is incorrect, make file empty
                if (e is JsonException)
                {
                    if (sr != null)
                    {
                        sr.Dispose();
                    }
                    TryFlushAsync(null);
                }
                return false;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Dispose();
                }
            }
            return true;
        }

        private bool WriteToStream(ScheduleInfo info)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(new FileStream(SurveySchedulePath, FileMode.Create, FileAccess.Write, FileShare.None));
                sw.Write(info == null ? string.Empty : JsonConvert.SerializeObject(info));
            }
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException)
                {
                    this.IgnoreSchedule = true;
                }
                return false;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
            return true;
        }

        private void TryFlush(ScheduleInfo info, bool outOfCycleFlush = false)
        {
            if (outOfCycleFlush)
            {
                WriteToStream(info);
                return;
            }
            int beforeExchange = Interlocked.CompareExchange(ref FlushCount, 0, FlushFrequecy);
            if(beforeExchange < FlushFrequecy)
            {
                Interlocked.Add(ref FlushCount, 1);
            }
            else if (beforeExchange > FlushFrequecy)
            {
                Interlocked.Exchange(ref FlushCount, 0);
            }
            else
            {
                if (!WriteToStream(info))
                {
                    Interlocked.Exchange(ref FlushCount, beforeExchange);
                }
            }
        }

        private async void TryFlushAsync(ScheduleInfo info, bool outOfCycleFlush = false)
        {
            await Task.Run(() =>
            {
                TryFlush(info, outOfCycleFlush);
            });
        }
    }
}
