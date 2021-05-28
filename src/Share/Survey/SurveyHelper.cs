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
    using Condition = Func<SurveyHelper, string, int, bool>;
    using Act = Action<SurveyHelper, string, int>;

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

        private static string SurveySchedulePath = Constants.SurveyScheduleInfoFile;
        private static string Predictor = Constants.Predictor;

        private DateTime LastPromptDate { get; set; }

        private ConcurrentDictionary<string, ModuleInfo> InternalMap { get; set; }

        private bool IgnoreSchedule;

        private bool IsDisabledFromEnv => "Disabled".Equals(Environment.GetEnvironmentVariable(Constants.AzurePSInterceptSurvey), StringComparison.OrdinalIgnoreCase);

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

            ScheduleInfo info = null;
            int majorVersion = moduleVersion.Major;

            if (InternalMap.Count == 0)
            {
                ReadFromStream(out info);
            }

            if (ShouldInternalUpdate(moduleName, majorVersion, InternalNotContains, Skip) 
                && ReadFromStream(out info) 
                && ShouldInternalUpdate(moduleName, majorVersion, InternalNotContains, UpdateInternalNotContains))
            {
                TryFlushAsync(info);
                return false;
            }

            //LastPromptDate.CompareTo(DateTime.MinValue) > 0 means survey is locked, otherwise lock free
            if (LastPromptDate > DateTime.MinValue && Today > LastPromptDate.AddDays(LockExpiredDays))
            {
                LastPromptDate = DateTime.MinValue;
            }

            if (ShouldInternalUpdate(moduleName, majorVersion, ShouldInternalPrompt, Skip)
                && ReadFromStream(out info)
                && ShouldInternalUpdate(moduleName, majorVersion, ShouldInternalPrompt, UpdateInternalPrompt))
            {
                TryFlushAsync(info);
                return true;
            }

            if ((ShouldInternalUpdate(moduleName, majorVersion, ShouldInternalBumpVersion, Skip) 
                    || ShouldInternalUpdate(moduleName, majorVersion, ShouldInternalCount, Skip)
                    || ShouldInternalUpdate(moduleName, majorVersion, IsInternalCountExpired, Skip)
                    || ShouldInternalUpdate(moduleName, majorVersion, IsInternalPromptExpired, Skip))
                && ReadFromStream(out info)
                && (ShouldInternalUpdate(moduleName, majorVersion, ShouldInternalBumpVersion, UpdateInternalBumpVersion)
                    || ShouldInternalUpdate(moduleName, majorVersion, ShouldInternalCount, UpdateInternalCount)
                    || ShouldInternalUpdate(moduleName, majorVersion, IsInternalCountExpired, UpdateInternalCountExpired)
                    || ShouldInternalUpdate(moduleName, majorVersion, IsInternalPromptExpired, UpdateInternalPromptExpired)))
            {
                TryFlushAsync(info);
            }
            return false;
        }

        private bool ShouldInternalUpdate(string moduleName, int majorVersion, Condition condition, Act updateInternal)
        {
            if (condition.Invoke(this, moduleName, majorVersion))
            {
                updateInternal.Invoke(this, moduleName, majorVersion);
            }
            return false;
        }

        private void Skip(SurveyHelper helper, string moduleName, int majorVersion) { }

        private bool InternalNotContains(SurveyHelper helper, string moduleName, int majorVersion) => !helper.InternalMap.ContainsKey(moduleName);

        private void UpdateInternalNotContains(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.InternalMap[moduleName] = new ModuleInfo()
            {
                Name = moduleName,
                MajorVersion = majorVersion,
                ActiveDays = 1,
                FirstActiveDate = CurrentDate,
                LastActiveDate = CurrentDate,
                Enabled = true
            };
        }

        private bool ShouldInternalBumpVersion(SurveyHelper helper, string moduleName, int majorVersion) 
            => majorVersion > helper.InternalMap[moduleName].MajorVersion;

        private void UpdateInternalBumpVersion(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.InternalMap[moduleName].MajorVersion = majorVersion;
            helper.InternalMap[moduleName].FirstActiveDate = CurrentDate;
            helper.InternalMap[moduleName].LastActiveDate = CurrentDate;
            helper.InternalMap[moduleName].ActiveDays = 1;
        }

        private bool ShouldInternalCount(SurveyHelper helper, string moduleName, int majorVersion) 
            => helper.InternalMap[moduleName].MajorVersion == majorVersion 
            && helper.InternalMap[moduleName].ActiveDays < SurveyTriggerCount 
            && helper.Today > Convert.ToDateTime(helper.InternalMap[moduleName].LastActiveDate)
            && helper.Today <= Convert.ToDateTime(helper.InternalMap[moduleName].FirstActiveDate).AddDays(CountExpiredDays);

        private void UpdateInternalCount(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.InternalMap[moduleName].ActiveDays += 1;
            helper.InternalMap[moduleName].LastActiveDate = CurrentDate;
        }

        private bool IsInternalCountExpired(SurveyHelper helper, string moduleName, int majorVersion)
            => helper.InternalMap[moduleName].MajorVersion == majorVersion
            && helper.InternalMap[moduleName].ActiveDays < SurveyTriggerCount
            && helper.Today > Convert.ToDateTime(helper.InternalMap[moduleName].LastActiveDate)
            && helper.Today > Convert.ToDateTime(helper.InternalMap[moduleName].FirstActiveDate).AddDays(CountExpiredDays);

        private void UpdateInternalCountExpired(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.InternalMap[moduleName].FirstActiveDate = CurrentDate;
            helper.InternalMap[moduleName].LastActiveDate = CurrentDate;
            helper.InternalMap[moduleName].ActiveDays = 1;
        }

        private bool ShouldInternalPrompt(SurveyHelper helper, string moduleName, int majorVersion)
            => helper.InternalMap[moduleName].MajorVersion == majorVersion
            && ((helper.InternalMap[moduleName].ActiveDays == SurveyTriggerCount && helper.LastPromptDate == DateTime.MinValue)
                 || helper.InternalMap[moduleName].ActiveDays == SurveyTriggerCount + 1 && helper.LastPromptDate == Convert.ToDateTime(helper.InternalMap[moduleName].LastActiveDate) && helper.Today == helper.LastPromptDate.AddDays(DelayForSecondPrompt)
                 || helper.InternalMap[moduleName].ActiveDays == SurveyTriggerCount + 2 && helper.LastPromptDate == Convert.ToDateTime(helper.InternalMap[moduleName].LastActiveDate) && helper.Today == helper.LastPromptDate.AddDays(DelayForThirdPrompt));

        private void UpdateInternalPrompt(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.LastPromptDate = Today;
            helper.InternalMap[moduleName].LastActiveDate = CurrentDate;
            helper.InternalMap[moduleName].ActiveDays += 1;
        }

        private bool IsInternalPromptExpired(SurveyHelper helper, string moduleName, int majorVersion)
            => helper.InternalMap[moduleName].MajorVersion == majorVersion
            && (InternalMap[moduleName].ActiveDays == SurveyTriggerCount + 1 && helper.LastPromptDate == Convert.ToDateTime(InternalMap[moduleName].LastActiveDate) && helper.Today > helper.LastPromptDate.AddDays(DelayForSecondPrompt)
                || InternalMap[moduleName].ActiveDays == SurveyTriggerCount + 2 && helper.LastPromptDate == Convert.ToDateTime(InternalMap[moduleName].LastActiveDate) && helper.Today > LastPromptDate.AddDays(DelayForThirdPrompt));

        private void UpdateInternalPromptExpired(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.InternalMap[moduleName].ActiveDays = 0;
        }

        private ScheduleInfo MergeScheduleInfo(ScheduleInfo externalScheduleInfo)
        {
            DateTime externalLastPromptDate = Convert.ToDateTime(externalScheduleInfo?.LastPromptDate);
            ScheduleInfo tmp = new ScheduleInfo() { Modules = new List<ModuleInfo>() };
            IDictionary<string, ModuleInfo> externalMap = new Dictionary<string, ModuleInfo>();
            foreach(ModuleInfo info in externalScheduleInfo?.Modules)
            {
                externalMap[info.Name] = info;
            }

            HashSet<string> moduleNames = new HashSet<string>(InternalMap.Keys);
            moduleNames.UnionWith(new HashSet<string>(externalMap.Keys));

            foreach (string name in moduleNames)
            {
                ModuleInfo item = null;
                if (InternalMap.ContainsKey(name) && (!externalMap.ContainsKey(name) || Convert.ToDateTime(InternalMap[name].LastActiveDate) > Convert.ToDateTime(externalMap[name].LastActiveDate)))
                {
                    item = new ModuleInfo(InternalMap[name]);
                }
                else
                {
                    if (externalLastPromptDate != DateTime.MinValue && Convert.ToDateTime(externalMap[name].LastActiveDate) == externalLastPromptDate)
                    {
                        LastPromptDate = externalLastPromptDate;
                    }
                    item = new ModuleInfo(externalMap[name]);
                    InternalMap[name] = item;
                }
                tmp.Modules.Add(item);
            }
            tmp.LastPromptDate = LastPromptDate.ToString("yyyy-MM-dd");
            return tmp;
        }

        private bool ReadFromStream(out ScheduleInfo info)
        {
            StreamReader sr = null;
            info = new ScheduleInfo();
            try
            {
                if (File.Exists(SurveySchedulePath))
                {
                    sr = new StreamReader(new FileStream(SurveySchedulePath, FileMode.Open, FileAccess.Read, FileShare.None));
                    int size = (int)sr.BaseStream.Length;
                    char[] buffer = new char[size];
                    sr.ReadBlock(buffer, 0, size);
                    ScheduleInfo tmp = JsonConvert.DeserializeObject<ScheduleInfo>(new string(buffer));
                    info = MergeScheduleInfo(tmp);
                }
                else
                {
                    info = MergeScheduleInfo(null);
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
