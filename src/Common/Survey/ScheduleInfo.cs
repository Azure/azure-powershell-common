using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.Common.Survey
{
    public class ScheduleInfo
    {
        [JsonProperty(PropertyName = "lastPromptDate")]
        internal string LastPromptDate { get; set; }

        [JsonProperty(PropertyName = "modules")]
        internal IList<ModuleInfo> Modules { get; set; }

        [JsonProperty(PropertyName = "propmptTimes")]
        internal int PromptTimes { get; set; }

        public void MergeScheduleInfo(ScheduleInfo externalScheduleInfo) { }
    }
}
