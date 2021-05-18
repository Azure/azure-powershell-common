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
    }
}
