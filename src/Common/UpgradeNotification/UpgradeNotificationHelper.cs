﻿using Microsoft.Azure.PowerShell.Common.Config;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Threading;

namespace Microsoft.Azure.PowerShell.Common.UpgradeNotification
{
    public class UpgradeNotificationHelper
    {
        private const string FrequencyKeyForUpgradeNotification = "VersionUpgradeNotification";
        private static TimeSpan FrequencyTimeSpanForUpgradeNotification = TimeSpan.FromDays(30);

        private const string FrequencyKeyForUpgradeCheck = "VersionUpgradeCheck";
        private static TimeSpan FrequencyTimeSpanForUpgradeCheck = TimeSpan.FromDays(2);
        //temp record file for az module versions
        private static string AzVersionCacheFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".Azure", "AzModuleVerions.json");
        private bool hasNotified { get; set; }
        private Dictionary<string, string> versionDict = null;

        private static UpgradeNotificationHelper _instance;

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

        public void WriteWarningMessageForVersionUpgrade(AzurePSCmdlet cmdlet, AzurePSQoSEvent _qosEvent, IConfigManager configManager, IFrequencyService frequencyService) {
            // skip if it's the exceptional case.
            if (cmdlet == null || _qosEvent == null || configManager == null || frequencyService == null) {
                return;
            }
            try
            {
                _qosEvent.HigherVersionsChecked = false;
                _qosEvent.UpgradeNotificationPrompted = false;

                //disabled by az config, skip
                if (configManager.GetConfigValue<bool>(ConfigKeysForCommon.CheckForUpgrade).Equals(false))
                {
                    return;
                }

                //has done check this session, skip
                if (hasNotified)
                {
                    return;
                }

                frequencyService.Register(FrequencyKeyForUpgradeCheck, FrequencyTimeSpanForUpgradeCheck);
                frequencyService.Register(FrequencyKeyForUpgradeNotification, FrequencyTimeSpanForUpgradeNotification);

                string checkModuleName = "Az";
                string checkModuleCurrentVersion = _qosEvent.AzVersion;
                string upgradeModuleNames = "Az";
                if ("0.0.0".Equals(_qosEvent.AzVersion))
                {
                    checkModuleName = _qosEvent.ModuleName;
                    checkModuleCurrentVersion = _qosEvent.ModuleVersion;
                    upgradeModuleNames = "Az.*";
                }

                //refresh az module versions if necessary
                frequencyService.TryRun(FrequencyKeyForUpgradeCheck, () => true, () =>
                {
                    Thread loadHigherVersionsThread = new Thread(new ThreadStart(() =>
                    {
                        _qosEvent.HigherVersionsChecked = true;
                        try
                        {
                            //no lock for this method, may skip some notifications, it's expected.
                            RefreshVersionInfo(upgradeModuleNames);
                        }
                        catch (Exception)
                        {
                            //do nothing
                        }
                    }));
                    loadHigherVersionsThread.Start();
                });

                bool shouldPrintWarningMsg = HasHigherVersion(checkModuleName, checkModuleCurrentVersion);

                //prompt warning message for upgrade if necessary
                frequencyService.TryRun(FrequencyKeyForUpgradeNotification, () => shouldPrintWarningMsg, () =>
                {
                    _qosEvent.UpgradeNotificationPrompted = true;
                    hasNotified = true;

                    string latestModuleVersion = GetModuleLatestVersion(checkModuleName);
                    string updateModuleCmdletName = GetCmdletForUpdateModule();
                    string warningMsg = string.Format(Resources.VersionUpgradeMessage, checkModuleName, checkModuleCurrentVersion, latestModuleVersion, updateModuleCmdletName, upgradeModuleNames);
                    if ("Az".Equals(checkModuleName) && GetInstance().HasHigherMajorVersion(checkModuleName, checkModuleCurrentVersion))
                    {
                        warningMsg += Environment.NewLine;
                        warningMsg += string.Format(Resources.BreakingChangesMessage, checkModuleCurrentVersion, latestModuleVersion, Resources.MigrationGuideLink);
                    }
                    cmdlet.WriteWarning(warningMsg);
                });
            }
            catch (Exception ex)
            {
                cmdlet.WriteDebug($"Failed to write warning message for version upgrade due to '{ex.Message}'.");
            }
        }

        private void RefreshVersionInfo(string loadModuleNames)
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

        private string GetModuleLatestVersion(string moduleName)
        {
            string defaultVersion = "0.0.0";
            if (!VersionsAreFreshed())
            {
                return defaultVersion;
            }
            return versionDict.ContainsKey(moduleName) ? versionDict[moduleName] : defaultVersion;
        }

        private bool HasHigherVersion(string moduleName, string currentVersion)
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

        private bool HasHigherMajorVersion(string moduleName, string currentVersion)
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

        private static string GetCmdletForUpdateModule()
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

        // This method is copied from CmdletExtensions.ExecuteScript. But it'll run with NewRunspace, ignore the warning or error message.
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
