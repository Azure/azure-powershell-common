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
    using Condition = Func<SurveyHelper, string, int, bool>;
    using UpdateModule = Action<SurveyHelper, string, int>;

    public class SurveyHelper
    {
        private const int _countExpiredDays = 30;
        private const int _lockExpiredDays = 30;
        private const int _surveyTriggerCount = 3;
        private const int _flushFrequecy = 5;
        private const int _delayForSecondPrompt = 2;
        private const int _delayForThirdPrompt = 5;
        private const int _activeDaysLimit = 3;
        private const int _promptLockDay = 180;

        private static SurveyHelper _instance;

        private int _flushCount;

        private static string SurveyScheduleInfoFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".Azure", "AzureRmSurvey.json");

        private const string _azureSurveyMessage = "AzSurveyMessage";
        private const string _azurePSInterceptSurvey = "Azure_PS_Intercept_Survey";
        

        private const string _predictor = "Az.Predictor";

        private DateTime LastPromptDate { get; set; }

        private DateTime LastActiveDay { get; set; }

        private DateTime ExpectedDate { get; set; }

        private int ActiveDays { get; set; }

        private int StartUsingDay { get; set; }    



        private ConcurrentDictionary<string, ModuleInfo> Modules { get; }

        private bool _ignoreSchedule;                                    
                                        
        private int AZActiveDays { get; set; }

        private String StartUsingDate { get; set; } 

        private DateTime ExpectedPromptDate { get; set; } 

        public string CurrentDate { get; }

        public DateTime Today { get; }


        private SurveyHelper()
        {
            CurrentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            Today = Convert.ToDateTime(CurrentDate);
            _ignoreSchedule = false;
            LastPromptDate = DateTime.MinValue;
            ExpectedDate = DateTime.MinValue;
            Modules = new ConcurrentDictionary<string, ModuleInfo>();
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
        public bool ShouldPropmtAzSurvey(String installationId){
            InitialSurveyHelper();
            if (_ignoreSchedule)
            {
                return false;
            }
            if (ExpectedDate != DateTime.MinValue && Today > Convert.ToDateTime(ExpectedDate))
            {
                ExpectedDate = Today.AddDays(180);
                LastPromptDate = Today;
                WriteToStream(JsonConvert.SerializeObject(GetScheduleInfo()));
                return true;
            }

            if (ExpectedDate == DateTime.MinValue && Today > Convert.ToDateTime(LastActiveDay)) 
            {
                LastActiveDay = Today;
                ActiveDays ++;
                if (ActiveDays >= _activeDaysLimit){
                    long st = System.Int64.Parse(installationId.Substring(25), System.Globalization.NumberStyles.HexNumber);
                    int RandomGapDay = (int) (st % 180 + 1);
                    ExpectedDate = Today.AddDays(RandomGapDay);
                }
                WriteToStream(JsonConvert.SerializeObject(GetScheduleInfo()));
            }
            return false;
        }

        private void InitialSurveyHelper()
        {
            StreamReader sr = null;
            if (File.Exists(SurveyScheduleInfoFile))
            {
                sr = new StreamReader(new FileStream(SurveyScheduleInfoFile, FileMode.Open, FileAccess.Read, FileShare.None));                    
                ScheduleInfo scheduleInfo = JsonConvert.DeserializeObject<ScheduleInfo>(sr.ReadToEnd());
                sr.Close();
                LastActiveDay = Convert.ToDateTime(scheduleInfo.LastActiveDay);
                ActiveDays = scheduleInfo.ActiveDays;
                ExpectedDate = Convert.ToDateTime(scheduleInfo.ExpectedDate);
                LastPromptDate = Convert.ToDateTime(scheduleInfo.LastPromptDate);
                return;
            }
            LastActiveDay = Today;
            LastPromptDate = Today;
            ExpectedDate = DateTime.MinValue;
            ActiveDays = 1;
            WriteToStream(JsonConvert.SerializeObject(GetScheduleInfo()));
        }

        private ScheduleInfo GetScheduleInfo()
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
