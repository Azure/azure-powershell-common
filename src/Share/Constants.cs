using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Azure.PowerShell.Share
{
    public class Constants
    {
        public static string ProfileDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".Azure");

        public static string SurveyScheduleInfoFile = Path.Combine(ProfileDirectory, "AzureRmSurvey.json");

        public const string AzurePSInterceptSurvey = "Azure_PS_Intercept_Survey";

        public const string Predictor = "Az.Predictor";
    }
}
