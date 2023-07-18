using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;

namespace Microsoft.Azure.PowerShell.Common.Share.UpgradeNotification
{
    public class UpgradeNotificationHelper
    {
        public const string FrequencyKeyForUpgradeNotification = "VersionUpgradeNotification";
        public static TimeSpan FrequencyTimeSpanForUpgradeNotification = TimeSpan.FromDays(30);

        public const string FrequencyKeyForUpgradeCheck = "VersionUpgradeCheck";
        public static TimeSpan FrequencyTimeSpanForUpgradeCheck = TimeSpan.FromDays(2);
        //temp record file for az module versions
        private static string AzVersionCacheFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".Azure", "AzModuleVerions.json");

        private static UpgradeNotificationHelper _instance;

        public bool hasNotified { get; set; }
        private Dictionary<string, string> versionDict = null;

        private UpgradeNotificationHelper()
        {
            try
            {
                // load temp record file to versionDict
                if (File.Exists(AzVersionCacheFile))
                {
                    using (StreamReader sr = new StreamReader(new FileStream(AzVersionCacheFile, FileMode.Open, FileAccess.Read, FileShare.None)))
                    {
                        versionDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
                    }
                }
            }
            catch (Exception)
            {
                versionDict = null;
            }
        }

        public static UpgradeNotificationHelper GetInstance()
        {
            if (_instance == null)
            {
                _instance = new UpgradeNotificationHelper();
            }
            return _instance;
        }


        public void RefreshVersionInfo(string loadModuleNames)
        {
            this.versionDict = LoadHigherAzVersions(loadModuleNames);
            if (!VersionsAreFreshed())
            {
                return;
            }
            string content = JsonConvert.SerializeObject(this.versionDict);
            using (StreamWriter sw = new StreamWriter(new FileStream(AzVersionCacheFile, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                sw.Write(content);
            }
        }

        private bool VersionsAreFreshed()
        {
            return versionDict != null && versionDict.Count > 0;
        }

        public string GetModuleLatestVersion(string moduleName)
        {
            string defaultVersion = "0.0.0";
            if (!VersionsAreFreshed())
            {
                return defaultVersion;
            }
            return versionDict.ContainsKey(moduleName) ? versionDict[moduleName] : defaultVersion;
        }

        public bool HasHigherVersion(string moduleName, string currentVersion)
        {
            if (!VersionsAreFreshed())
            {
                return false;
            }
            try
            {
                Version currentVersionValue = Version.Parse(currentVersion);
                Version latestVersionValue = Version.Parse(versionDict[moduleName]);
                return latestVersionValue > currentVersionValue;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool HasHigherMajorVersion(string moduleName, string currentVersion)
        {
            if (!VersionsAreFreshed())
            {
                return false;
            }
            try
            {
                Version currentVersionValue = Version.Parse(currentVersion);
                Version latestVersionValue = Version.Parse(versionDict[moduleName]);
                return latestVersionValue.Major > currentVersionValue.Major;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Dictionary<string, string> LoadHigherAzVersions(string moduleName)
        {
            Dictionary<string, string> versionDict = new Dictionary<string, string>();

            string findModuleCmdlet = GetCmdletForFindModule();
            findModuleCmdlet += " -Name " + moduleName + " | Select-Object Name, Version";

            var outputs = ExecutePSScript<PSObject>(findModuleCmdlet);
            foreach (PSObject obj in outputs)
            {
                versionDict[obj.Properties["Name"].Value.ToString()] = obj.Properties["Version"].Value.ToString();
            }
            return versionDict;
        }

        public static string GetCmdletForUpdateModule()
        {
            if (ExecutePSScript<PSObject>("Get-Command -Name Update-PSResource").Count > 0)
            {
                return "Update-PSResource";
            }
            else
            {
                return "Update-Module";
            }
        }

        private static string GetCmdletForFindModule()
        {
            if (ExecutePSScript<PSObject>("Get-Command -Name Find-PSResource").Count > 0)
            {
                return "Find-PSResource -Repository PSGallery -Type Module";
            }
            else
            {
                return "Find-Module -Repository PSGallery";
            }
        }

        private static List<T> ExecutePSScript<T>(string contents)
        {
            List<T> output = new List<T>();

            using (System.Management.Automation.PowerShell powershell = System.Management.Automation.PowerShell.Create(RunspaceMode.NewRunspace))
            {
                powershell.AddScript(contents);
                Collection<T> result = powershell.Invoke<T>();
                if (result != null && result.Count > 0)
                {
                    output.AddRange(result);
                }
            }

            return output;
        }
    }
}
