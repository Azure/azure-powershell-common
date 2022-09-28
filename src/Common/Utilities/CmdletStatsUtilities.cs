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

using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.WindowsAzure.Commands.Common.Utilities
{
    public static class CmdletStatsUtilities
    {
        private const string ReportHeaderCommandName = "CommandName";
        private const string ReportHeaderParameterSetName = "ParameterSetName";
        private const string ReportHeaderParameters = "Parameters";
        private const string ReportHeaderSourceScript = "SourceScript";
        private const string ReportHeaderScriptLineNumber = "LineNumber";
        private const string Delimiter = ",";

        private static readonly string CmdletStatsOutputRootFolder;

        private static readonly bool IsWindowsPlatform;

        private static readonly IList<string> ExcludedSource = new List<string>
        {
            "Common.ps1",
            "Assert.ps1",
            "AzureRM.Resources.ps1",
            "AzureRM.Storage.ps1"
        };

        private static ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        static CmdletStatsUtilities()
        {
            IsWindowsPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var repoRootFolder = ProbeRepoDirectory();
            if (!string.IsNullOrEmpty(repoRootFolder))
            {
                CmdletStatsOutputRootFolder = Path.Combine(repoRootFolder, "artifacts", "CmdletStatisticsAnalysis", "Raw");
                DirectoryInfo rawDir = new DirectoryInfo(CmdletStatsOutputRootFolder);
                if (!rawDir.Exists)
                {
                    Directory.CreateDirectory(CmdletStatsOutputRootFolder);
                }
            }
        }

        private static string ProbeRepoDirectory()
        {
            string directoryPath = "..";
            while (Directory.Exists(directoryPath) && (!Directory.Exists(Path.Combine(directoryPath, "src")) || !Directory.Exists(Path.Combine(directoryPath, "artifacts"))))
            {
                directoryPath = Path.Combine(directoryPath, "..");
            }

            string result = Directory.Exists(directoryPath) ? Path.GetFullPath(directoryPath) : null;
            return result;
        }

        private static string GenerateCsvHeader()
        {
            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.Append(ReportHeaderCommandName).Append(Delimiter)
                         .Append(ReportHeaderParameterSetName).Append(Delimiter)
                         .Append(ReportHeaderParameters).Append(Delimiter)
                         .Append(ReportHeaderSourceScript).Append(Delimiter)
                         .Append(ReportHeaderScriptLineNumber);

            return headerBuilder.ToString();
        }

        private static string GenerateCsvRecord(string commandName, string parameterSetName, string parameters, string sourceScript, int scriptLineNumber)
        {
            StringBuilder recordBuilder = new StringBuilder();
            recordBuilder.Append(commandName).Append(Delimiter)
                         .Append(parameterSetName).Append(Delimiter)
                         .Append(parameters).Append(Delimiter)
                         .Append(sourceScript).Append(Delimiter)
                         .Append(scriptLineNumber);

            return recordBuilder.ToString();
        }

        public static void LogCmdletStatistics(string moduleName, string commandName, string parameterSetName, string parameters, string sourceScript, int scriptLineNumber)
        {
            if (!IsWindowsPlatform || string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(commandName) || ExcludedSource.Contains(sourceScript))
                return;

            var pattern = @"\\(?:artifacts\\Debug|src)\\(?:Az\.)?(?<ModuleName>[a-zA-Z]+)\\";
            var match = Regex.Match(sourceScript, pattern, RegexOptions.IgnoreCase);
            var testingModuleName = $"Az.{match.Groups["ModuleName"].Value}";
            if (string.Compare(testingModuleName, moduleName, true) != 0)
                return;

            var csvFilePath = Path.Combine(CmdletStatsOutputRootFolder, $"{moduleName}.csv");
            bool csvExists = File.Exists(csvFilePath);
            using (var csvFileStream = new FileStream(csvFilePath, csvExists ? FileMode.Append : FileMode.CreateNew, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(csvFileStream))
                {
                    var csvRecord = GenerateCsvRecord(commandName, parameterSetName, parameters, Path.GetFileName(sourceScript), scriptLineNumber);
                    if (csvExists)
                    {
                        streamWriter.WriteLine();
                        streamWriter.Write(csvRecord);
                    }
                    else
                    {
                        var csvHeader = GenerateCsvHeader();
                        streamWriter.WriteLine(csvHeader);
                        streamWriter.Write(csvRecord);
                    }
                }
            }
        }
    }
}
