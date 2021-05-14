using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.Common.Survey
{
    public class ModuleInfo
    {
        [JsonProperty(PropertyName = "name")]
        internal string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        internal int Version { get; set; }

        [JsonProperty(PropertyName = "count")]
        internal int Count { get; set; }

        [JsonProperty(PropertyName = "firstActiveDate")]
        internal string FirstActiveDate { get; set; }

        [JsonProperty(PropertyName = "lastActiveDate")]
        internal string LastActiveDate { get; set; }

        [JsonProperty(PropertyName = "deprecatedVersions")]
        internal int DeprecatedVersions { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        internal bool Enabled { get; set; }

        internal ModuleInfo(ModuleInfo info)
        {
            Name = info.Name;
            Version = info.Version;
            Count = info.Count;
            FirstActiveDate = info.FirstActiveDate;
            LastActiveDate = info.LastActiveDate;
            DeprecatedVersions = info.DeprecatedVersions;
            Enabled = info.Enabled;
        }

        internal ModuleInfo() { }

        internal bool IsDeprecatedVersion(int version)
        {
            string str = Convert.ToString(DeprecatedVersions, 2);
            return version == 0 ? false : str[str.Length - version].Equals('1');
        }

        internal void Deprecate(string date)
        {
            DeprecateVersion();
            FirstActiveDate = date;
            LastActiveDate = date;
            Count = 0;
        }

        private void DeprecateVersion()
        {
            string str = Convert.ToString(DeprecatedVersions, 2);
            if (Version > str.Length)
            {
                string prefix = "";
                for (int i = 0; i < Version - str.Length; i++)
                {
                    prefix += "0";
                }
                str = prefix + str;
            }
            char[] arr = str.ToCharArray();
            arr[arr.Length - Version] = '1';
            int two = 1;
            DeprecatedVersions = 0;
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if ('1'.Equals(arr[i]))
                {
                    DeprecatedVersions += two;
                }
                two *= 2;
            }
            Version = 0;
        }
    }
}
