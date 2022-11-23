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
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Microsoft.Azure.PowerShell.Common.Share.Survey
{
    public class SurveyHelper
    {
        private const int _activeDaysLimit = 3;
        private const int _promptLockDay = 180;

        private static SurveyHelper _instance;

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

        private int ExecuteCmdletCount { get; set; }


        private SurveyHelper()
        {
            CurrentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            Today = Convert.ToDateTime(CurrentDate);
            _ignoreSchedule = false;
            LastPromptDate = DateTime.MinValue;
            ExpectedDate = DateTime.MinValue;
            ExecuteCmdletCount = 0;
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
            ExecuteCmdletCount = ExecuteCmdletCount + 1;
            if (ExecuteCmdletCount < 3)
            {
                return false;
            }
            if (ExpectedDate != DateTime.MinValue && Today >= ExpectedDate)
            {
                ExpectedDate = Today.AddDays(_promptLockDay);
                LastPromptDate = Today;
                WriteToStream(JsonConvert.SerializeObject(GetScheduleInfo()));
                return true;
            }
            return false;
        }

        [Obsolete("The method is deprecated, please use ShouldPromptAzSurvey() instead.")]
        public bool ShouldPropmtSurvey(string moduleName, Version moduleVersion){
            return ShouldPromptAzSurvey();
        }

        public void updateSurveyHelper(string installationId){
            InitialSurveyHelper();
            if (ExpectedDate == DateTime.MinValue && Today > LastActiveDay) 
            {
                LastActiveDay = Today;
                ActiveDays ++;
                if (ActiveDays >= _activeDaysLimit){
                    Guid insGuid;
                    if(!Guid.TryParse(installationId, out insGuid))
                    {
                        insGuid = Guid.NewGuid();
                    }
                    int RandomGapDay = insGuid.ToByteArray()[15] & 127;
                    ExpectedDate = Today.AddDays(RandomGapDay);
                    LastPromptDate = Today;
                    LastActiveDay = DateTime.MinValue;
                    ActiveDays = -1;
                }
                WriteToStream(JsonConvert.SerializeObject(GetScheduleInfo()));
            }
        }

        private void InitialSurveyHelper()
        {
            StreamReader sr = null;
            ScheduleInfo scheduleInfo = null;
            try 
            {
                if (File.Exists(SurveyScheduleInfoFile))
                {
                    sr = new StreamReader(new FileStream(SurveyScheduleInfoFile, FileMode.Open, FileAccess.Read, FileShare.None));                    
                    scheduleInfo = JsonConvert.DeserializeObject<ScheduleInfo>(sr.ReadToEnd());
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
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException)
                {
                    _ignoreSchedule = true;
                }
                //deserialize failed, means content of file is incorrect, make file empty
                if (e is JsonException)
                {
                    if (sr != null)
                    {
                        sr.Dispose();
                    }
                    Task.Run(() =>
                    {
                        WriteToStream(string.Empty);
                    });
                }
            }
            finally
            {
                if (sr != null)
                {
                    sr.Dispose();
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
                LastPromptDate = LastPromptDate.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo), 
                ActiveDays = ActiveDays, 
                LastActiveDay = LastActiveDay.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo),  
                ExpectedDate = ExpectedDate.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo),
                Modules = new List<ModuleInfo>()
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
