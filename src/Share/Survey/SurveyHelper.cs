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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.PowerShell.Share.Survey
{
    using Condition = Func<SurveyHelper, string, int, bool>;
    using UpdateModule = Action<SurveyHelper, string, int>;

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

        private ConcurrentDictionary<string, ModuleInfo> Modules { get; set; }

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
            Modules = new ConcurrentDictionary<string, ModuleInfo>();
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

            int majorVersion = moduleVersion.Major;

            if (Modules.Count == 0)
            {
                ReadFromStream();
            }

            if (ShouldFlush(moduleName, majorVersion, ShouldModuleAdd, ModuleAdd))
            {
                TryFlushAsync(GetModules());
                return false;
            }

            //LastPromptDate.CompareTo(DateTime.MinValue) > 0 means survey is locked, otherwise lock free
            if (LastPromptDate > DateTime.MinValue && Today > LastPromptDate.AddDays(LockExpiredDays))
            {
                LastPromptDate = DateTime.MinValue;
            }

            if (ShouldFlush(moduleName, majorVersion, ShouldAzPredictorPrompt, AzPredictorPrompt) 
                || ShouldFlush(moduleName, majorVersion, ShouldModulePrompt, ModulePrompt))
            {
                TryFlushAsync(GetModules(), true);
                return true;
            }

            if ((ShouldFlush(moduleName, majorVersion, ShouldModuleBumpVersion, ModuleBumpVersion)
                || ShouldFlush(moduleName, majorVersion, ShouldModuleCount, ModuleCount)
                || ShouldFlush(moduleName, majorVersion, ShouldModuleCountExpire, ModuleCountExpire)
                || ShouldFlush(moduleName, majorVersion, ShouldModulePromptExpire, ModulePromptExpire)))
            {
                TryFlushAsync(GetModules());
            }
            return false;
        }

        private bool ShouldFlush(string moduleName, int majorVersion, Condition condition, UpdateModule updateModule = null)
        {
            if (condition.Invoke(this, moduleName, majorVersion) && ReadFromStream() && condition.Invoke(this, moduleName, majorVersion))
            {
                updateModule.Invoke(this, moduleName, majorVersion);
                return true;
            }
            return false;
        }

        private bool ShouldModuleAdd(SurveyHelper helper, string moduleName, int majorVersion) => !helper.Modules.ContainsKey(moduleName);

        private void ModuleAdd(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.Modules[moduleName] = new ModuleInfo()
            {
                Name = moduleName,
                MajorVersion = majorVersion,
                ActiveDays = 1,
                FirstActiveDate = CurrentDate,
                LastActiveDate = CurrentDate,
                Enabled = true
            };
        }

        private bool ShouldModuleBumpVersion(SurveyHelper helper, string moduleName, int majorVersion) 
            => majorVersion > helper.Modules[moduleName].MajorVersion;

        private void ModuleBumpVersion(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.Modules[moduleName].MajorVersion = majorVersion;
            helper.Modules[moduleName].FirstActiveDate = CurrentDate;
            helper.Modules[moduleName].LastActiveDate = CurrentDate;
            helper.Modules[moduleName].ActiveDays = 1;
        }

        private bool ShouldModuleCount(SurveyHelper helper, string moduleName, int majorVersion) 
            => helper.Modules[moduleName].MajorVersion == majorVersion 
            && helper.Modules[moduleName].ActiveDays < SurveyTriggerCount 
            && helper.Today > Convert.ToDateTime(helper.Modules[moduleName].LastActiveDate)
            && helper.Today <= Convert.ToDateTime(helper.Modules[moduleName].FirstActiveDate).AddDays(CountExpiredDays);

        private void ModuleCount(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.Modules[moduleName].ActiveDays += 1;
            helper.Modules[moduleName].LastActiveDate = CurrentDate;
        }

        private bool ShouldModuleCountExpire(SurveyHelper helper, string moduleName, int majorVersion)
            => helper.Modules[moduleName].MajorVersion == majorVersion
            && helper.Modules[moduleName].ActiveDays < SurveyTriggerCount
            && helper.Today > Convert.ToDateTime(helper.Modules[moduleName].LastActiveDate)
            && helper.Today > Convert.ToDateTime(helper.Modules[moduleName].FirstActiveDate).AddDays(CountExpiredDays);

        private void ModuleCountExpire(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.Modules[moduleName].FirstActiveDate = CurrentDate;
            helper.Modules[moduleName].LastActiveDate = CurrentDate;
            helper.Modules[moduleName].ActiveDays = 1;
        }

        private bool ShouldModulePrompt(SurveyHelper helper, string moduleName, int majorVersion)
            => helper.Modules[moduleName].MajorVersion == majorVersion
            && ((helper.Modules[moduleName].ActiveDays == SurveyTriggerCount && helper.LastPromptDate == DateTime.MinValue)
                 || helper.Modules[moduleName].ActiveDays == SurveyTriggerCount + 1 && helper.LastPromptDate == Convert.ToDateTime(helper.Modules[moduleName].LastActiveDate) && helper.Today == helper.LastPromptDate.AddDays(DelayForSecondPrompt)
                 || helper.Modules[moduleName].ActiveDays == SurveyTriggerCount + 2 && helper.LastPromptDate == Convert.ToDateTime(helper.Modules[moduleName].LastActiveDate) && helper.Today == helper.LastPromptDate.AddDays(DelayForThirdPrompt));

        private void ModulePrompt(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.LastPromptDate = Today;
            helper.Modules[moduleName].LastActiveDate = CurrentDate;
            helper.Modules[moduleName].ActiveDays += 1;
        }

        private bool ShouldAzPredictorPrompt(SurveyHelper helper, string moduleName, int majorVersion)
            => Predictor.Equals(moduleName, StringComparison.OrdinalIgnoreCase)
            && helper.Modules[moduleName].MajorVersion == majorVersion
            && ((helper.Modules[moduleName].ActiveDays == SurveyTriggerCount && helper.Today <= Convert.ToDateTime(helper.Modules[moduleName].FirstActiveDate).AddDays(CountExpiredDays))
                 || helper.Modules[moduleName].ActiveDays == SurveyTriggerCount + 1 && helper.Today == Convert.ToDateTime(helper.Modules[moduleName].LastActiveDate).AddDays(DelayForSecondPrompt)
                 || helper.Modules[moduleName].ActiveDays == SurveyTriggerCount + 2 && helper.Today == Convert.ToDateTime(helper.Modules[moduleName].LastActiveDate).AddDays(DelayForThirdPrompt));

        private void AzPredictorPrompt(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.Modules[moduleName].LastActiveDate = CurrentDate;
            helper.Modules[moduleName].ActiveDays += 1;
        }

        private bool ShouldModulePromptExpire(SurveyHelper helper, string moduleName, int majorVersion)
            => helper.Modules[moduleName].MajorVersion == majorVersion
            && (Modules[moduleName].ActiveDays == SurveyTriggerCount + 1 && helper.LastPromptDate == Convert.ToDateTime(Modules[moduleName].LastActiveDate) && helper.Today > helper.LastPromptDate.AddDays(DelayForSecondPrompt)
                || Modules[moduleName].ActiveDays == SurveyTriggerCount + 2 && helper.LastPromptDate == Convert.ToDateTime(Modules[moduleName].LastActiveDate) && helper.Today > LastPromptDate.AddDays(DelayForThirdPrompt));

        private void ModulePromptExpire(SurveyHelper helper, string moduleName, int majorVersion)
        {
            helper.Modules[moduleName].ActiveDays = 0;
        }

        private void MergeScheduleInfo(ScheduleInfo externalScheduleInfo)
        {
            DateTime externalLastPromptDate = Convert.ToDateTime(externalScheduleInfo?.LastPromptDate);
            IDictionary<string, ModuleInfo> externalMap = new Dictionary<string, ModuleInfo>();
            foreach(ModuleInfo info in externalScheduleInfo?.Modules)
            {
                externalMap[info.Name] = info;
            }

            HashSet<string> moduleNames = new HashSet<string>(Modules.Keys);
            moduleNames.UnionWith(new HashSet<string>(externalMap.Keys));

            foreach (string name in moduleNames)
            {
                ModuleInfo item = null;
                if (Modules.ContainsKey(name) && (!externalMap.ContainsKey(name) || Convert.ToDateTime(Modules[name].LastActiveDate) > Convert.ToDateTime(externalMap[name].LastActiveDate)))
                {
                    item = new ModuleInfo(Modules[name]);
                }
                else
                {
                    if (externalLastPromptDate != DateTime.MinValue && Convert.ToDateTime(externalMap[name].LastActiveDate) == externalLastPromptDate)
                    {
                        LastPromptDate = externalLastPromptDate;
                    }
                    item = new ModuleInfo(externalMap[name]);
                    Modules[name] = item;
                }
            }
        }

        private ScheduleInfo GetModules()
        { 
            return new ScheduleInfo() { LastPromptDate = LastPromptDate.ToString("yyyy-MM-dd"), Modules = Modules.Values.ToList() };
        }

        private bool ReadFromStream()
        {
            StreamReader sr = null;
            try
            {
                if (File.Exists(SurveySchedulePath))
                {
                    sr = new StreamReader(new FileStream(SurveySchedulePath, FileMode.Open, FileAccess.Read, FileShare.None));
                    int size = (int)sr.BaseStream.Length;
                    char[] buffer = new char[size];
                    sr.ReadBlock(buffer, 0, size);
                    ScheduleInfo tmp = JsonConvert.DeserializeObject<ScheduleInfo>(new string(buffer));
                    MergeScheduleInfo(tmp);
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
