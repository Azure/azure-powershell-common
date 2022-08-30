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
using System.IO;
using System.Threading;

namespace Microsoft.Azure.PowerShell.Common.Share.Survey
{
    public class SurveyHelper
    {
        private const int _activeDaysLimit = 3;
        private const int _promptLockDay = 180;

        private static SurveyHelper _instance;

        private int _flushCount;

        private static string SurveyScheduleInfoFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".Azure", "AzureRmSurvey.json");
        

        private DateTime LastPromptDate { get; set; }

        private DateTime LastActiveDay { get; set; }

        private DateTime ExpectedDate { get; set; }

        private int ActiveDays { get; set; }

        private bool _ignoreSchedule;                                    
                                        
        public string CurrentDate { get; }

        public DateTime Today { get; }


        private SurveyHelper()
        {
            CurrentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            Today = Convert.ToDateTime(CurrentDate);
            _ignoreSchedule = false;
            LastPromptDate = DateTime.MinValue;
            ExpectedDate = DateTime.MinValue;
            Interlocked.Exchange(ref _flushCount, 0);
        }

        public static SurveyHelper GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SurveyHelper();
            }
            return _instance;
        }
        public bool ShouldPromptAzSurvey(){
            if (_ignoreSchedule)
            {
                return false;
            }
            if (ExpectedDate != DateTime.MinValue && Today > Convert.ToDateTime(ExpectedDate))
            {
                ExpectedDate = Today.AddDays(_promptLockDay);
                LastPromptDate = Today;
                WriteToStream(JsonConvert.SerializeObject(GetScheduleInfo()));
                return true;
            }
            return false;
        }

        public void updateSurveyHelper(string installationId){
            InitialSurveyHelper();
            if (ExpectedDate == DateTime.MinValue && Today > Convert.ToDateTime(LastActiveDay)) 
            {
                LastActiveDay = Today;
                ActiveDays ++;
                if (ActiveDays >= _activeDaysLimit){
                    Guid insGuid = new Guid(installationId);
                    int RandomGapDay = insGuid.ToByteArray()[15] & 127;
                    ExpectedDate = Today.AddDays(RandomGapDay);
                    LastPromptDate = Today;
                }
                WriteToStream(JsonConvert.SerializeObject(GetScheduleInfo()));
            }
        }

        private void InitialSurveyHelper()
        {
            if (File.Exists(SurveyScheduleInfoFile))
            {
                using (StreamReader sr = new StreamReader(new FileStream(SurveyScheduleInfoFile, FileMode.Open, FileAccess.Read, FileShare.None))) {
                    ScheduleInfo scheduleInfo = JsonConvert.DeserializeObject<ScheduleInfo>(sr.ReadToEnd());
                    LastActiveDay = Convert.ToDateTime(scheduleInfo.LastActiveDay);
                    DateTime date = DateTime.MinValue;
                    DateTime.TryParse(scheduleInfo.LastActiveDay, out date);
                    LastActiveDay = date;
                    ActiveDays = scheduleInfo.ActiveDays;
                    DateTime.TryParse(scheduleInfo.ExpectedDate, out date);
                    ExpectedDate = date;
                    DateTime.TryParse(scheduleInfo.LastPromptDate, out date);
                    LastPromptDate = date;
                    return;
                }
            }
            LastActiveDay = Today;
            LastPromptDate = Today;
            ExpectedDate = DateTime.MinValue;
            ActiveDays = 1;
            WriteToStream(JsonConvert.SerializeObject(GetScheduleInfo()));
        }

        public ScheduleInfo GetScheduleInfo()
        { 
            return new ScheduleInfo() 
            { 
                LastPromptDate = LastPromptDate.ToString("yyyy-MM-dd"), 
                ActiveDays = ActiveDays, 
                LastActiveDay = LastActiveDay.ToString("yyyy-MM-dd"),  
                ExpectedDate = ExpectedDate.ToString("yyyy-MM-dd")
            };
        }

        private bool WriteToStream(string info)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(new FileStream(SurveyScheduleInfoFile, FileMode.Create, FileAccess.Write, FileShare.None));
                sw.Write(info);
            }
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException)
                {
                    _ignoreSchedule = true;
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
    }
}
