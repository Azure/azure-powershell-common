using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.Common.Survey
{
    public class SurveyHelper
    {
        private const int _countExpiredDays = 30;
        private const int _lockExpiredDays = 30;
        private const int _surveyTriggerCount = 3;
        private const int _flushFrequecy = 5;
        private const int _maxConsecutivePromptTimes = 3;

        private SurveyHelper()
        {
            CurrentDate = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd");
            IgnoreSchedule = "Disabled".Equals(Environment.GetEnvironmentVariable(AzurePowerShell.AzurePSInterceptSurvey));
            LastPromptDate = DateTime.MinValue;
            InternalMap = new ConcurrentDictionary<string, ModuleInfo>();
            FlushCount = 0;
            PromptTimes = 0;
        }

        private static SurveyHelper _instance
        {
            get
            {
                if (_instance == null)
                {
                    return new SurveyHelper();
                }
                return _instance;
            }
        }

        private int FlushCount;

        private int PromptTimes;

        private static string SurveySchedulePath = AzurePowerShell.SurveyScheduleInfoDirectory;

        private DateTime LastPromptDate { get; set; }

        private IDictionary<string, ModuleInfo> InternalMap { get; set; }

        private bool IgnoreSchedule;

        private readonly string CurrentDate;

        public static SurveyHelper GetInstance()
        {
            return _instance;
        }

        public bool ShouldPropmtSurvey(string moduleName, string moduleVersion)
        {
            if (IgnoreSchedule)
            {
                return false;
            }

            int majorVersion = Int32.Parse(moduleVersion.Split('.')[0]);

            if (!InternalMap.ContainsKey(moduleName))
            {
                InternalMap[moduleName] = new ModuleInfo() {
                    Name = moduleName,
                    Version = majorVersion,
                    Count = 1,
                    FirstActiveDate = CurrentDate,
                    LastActiveDate = CurrentDate,
                    DeprecatedVersions = 0,
                    Enabled = true
                };
            }

            ModuleInfo cur = InternalMap[moduleName];

            //Lock.CompareTo(DateTime.MinValue) > 0 means survey is locked, otherwise lock free
            if (LastPromptDate.CompareTo(DateTime.MinValue) > 0 && Convert.ToDateTime(CurrentDate).CompareTo(LastPromptDate.AddDays(_lockExpiredDays)) > 0)
            {
                LastPromptDate = DateTime.MinValue;
            }

            //if version is not current version and not deprecated, start count for this version
            if (majorVersion != cur.Version && !cur.IsDeprecatedVersion(majorVersion))
            {
                cur.Version = majorVersion;
                cur.FirstActiveDate = CurrentDate;
                cur.LastActiveDate = CurrentDate;
                cur.Count = 1;
                TryFlush();
            }
            else if (majorVersion == cur.Version)
            {
                //prompt surey
                if (cur.Count == _surveyTriggerCount && LastPromptDate.CompareTo(DateTime.MinValue) == 0)
                {
                    if (TryPrompt(moduleName))
                    {
                        InternalMap[moduleName].Deprecate(CurrentDate);
                        LastPromptDate = Convert.ToDateTime(CurrentDate);
                        TryFlush();
                    }
                    return true;
                }
                else
                {
                    DateTime date = Convert.ToDateTime(CurrentDate);
                    //date is later than last active date and not expired
                    if (date.CompareTo(Convert.ToDateTime(cur.LastActiveDate)) > 0 && date.CompareTo(Convert.ToDateTime(cur.FirstActiveDate).AddDays(_countExpiredDays)) <= 0)
                    {
                        cur.Count += 1;
                        cur.LastActiveDate = CurrentDate;
                        TryFlush();
                    }
                    //count expired, start over again
                    else if (date.CompareTo(Convert.ToDateTime(cur.FirstActiveDate).AddDays(_countExpiredDays)) > 0)
                    {
                        cur.FirstActiveDate = CurrentDate;
                        cur.LastActiveDate = CurrentDate;
                        cur.Count = 1;
                        TryFlush();
                    }
                }
            }
            return false;
        }

        

        private bool MergeScheduleInfo(ScheduleInfo externalScheduleInfo, string moduleName, out bool prompt, out ScheduleInfo info)
        {
            bool hasDiff = false;
            bool shouldPrompt = false;
            DateTime externalLock = Convert.ToDateTime(externalScheduleInfo.LastPromptDate);
            if (LastPromptDate.CompareTo(externalLock) != 0 && externalLock.CompareTo(DateTime.MinValue) > 0 && externalLock.AddDays(_lockExpiredDays).CompareTo(Convert.ToDateTime(CurrentDate)) >= 0)
            {
                //no need to write to file if only lock changed
                //if current process is lock free, take external lock only if external lock is later than current date
                //if current process has lock and external is lock free, keep lock
                LastPromptDate = externalLock;
            }

            ScheduleInfo tmp = new ScheduleInfo() { LastPromptDate = LastPromptDate.ToString("yyyy'-'MM'-'dd"), Modules = new List<ModuleInfo>() };
            IDictionary<string, ModuleInfo> externalMap = new Dictionary<string, ModuleInfo>();
            externalScheduleInfo?.Modules?.ForEach<ModuleInfo>(x => externalMap[x.Name] = x);

            HashSet<string> moduleNames = new HashSet<string>(InternalMap.Keys);
            moduleNames.UnionWith(new HashSet<string>(externalMap.Keys));


            moduleNames.ForEach<string>(name =>
            {
                ModuleInfo item = null;
                if (InternalMap.ContainsKey(name) && (!externalMap.ContainsKey(name) || Convert.ToDateTime(InternalMap[name].LastActiveDate).CompareTo(Convert.ToDateTime(externalMap[name])) > 0))
                {
                    item = new ModuleInfo(InternalMap[name]);
                    if (moduleName != null && moduleName.Equals(name))
                    {
                        shouldPrompt = true;
                    }
                    hasDiff = true;
                }
                else
                {
                    item = new ModuleInfo(externalMap[name]);
                    InternalMap[name] = item;
                }
                tmp.Modules.Add(item);
            });

            info = tmp;
            prompt = shouldPrompt;

            return hasDiff;
        }

        private bool ReadFromStream(string moduleName, out bool prompt)
        {
            ScheduleInfo info = null;
            return ReadFromStream(moduleName, out prompt, out info);
        }

        private bool ReadFromStream(out ScheduleInfo info)
        {
            string name = null;
            bool prompt = false;
            return ReadFromStream(name, out prompt, out info);
        }

        private bool ReadFromStream(string moduleName, out bool prompt, out ScheduleInfo info)
        {
            FileStream fs = new FileStream(SurveySchedulePath, FileMode.Open, FileAccess.Read, FileShare.None);
            try
            {
                StringBuilder sb = new StringBuilder();
                long size = fs.Length;
                byte[] bytes = new byte[size];
                int index = 0;
                do
                {
                    int subSize = size > Int32.MaxValue ? Int32.MaxValue : (int)size;
                    if (subSize < bytes.Length)
                    {
                        bytes = new byte[subSize];
                    }
                    int count = subSize;
                    while (count > 0)
                    {
                        int segment = fs.Read(bytes, index, count);
                        if (segment == 0)
                        {
                            break;
                        }
                        index += segment;
                        count -= segment;
                    }
                    sb.Append(Encoding.Default.GetString(bytes));
                    size -= subSize;
                }
                while (size > 0);
                return MergeScheduleInfo(JsonConvert.DeserializeObject<ScheduleInfo>(sb.ToString()), moduleName, out prompt, out info);
            }
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException)
                {
                    this.IgnoreSchedule = true;
                }
                prompt = false;
                info = null;
                return false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
            }      
        }

        private bool WriteToStream(ScheduleInfo info)
        {
            if (info ==null)
            {
                return false;
            }
            FileStream fs = null;
            try
            {
                fs = new FileStream(SurveySchedulePath, FileMode.Create, FileAccess.Write, FileShare.None);
                byte[] bytes = Encoding.Default.GetBytes(JsonConvert.SerializeObject(info));
                int size = bytes.Length;
                int index = 0;
                do
                {
                    int subSize = size > Int32.MaxValue ? Int32.MaxValue : (int)size;
                    fs.Write(bytes, index, subSize);
                    index += subSize;
                    size -= subSize;
                }
                while (size > 0);
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
                if (fs != null)
                {
                    fs.Dispose();
                }
            }
            return true;
        }

        private void Flush(ScheduleInfo info)
        {
            if (FlushCount >= _flushFrequecy && WriteToStream(info))
            {
                Interlocked.Exchange(ref FlushCount, 0);
            }
            else
            {
                Interlocked.Add(ref FlushCount, 1);
            }
        }

        private async void TryFlush()
        {
            await Task.Run(() =>
            {
                ScheduleInfo info = null;
                if (!File.Exists(SurveySchedulePath) || ReadFromStream(out info))
                {
                    Flush(info);
                }
            });
        }

        private bool TryPrompt(string moduleName)
        {
            bool shouldPrompt = false;
            return ReadFromStream(moduleName, out shouldPrompt) && shouldPrompt;
        }
    }
}
