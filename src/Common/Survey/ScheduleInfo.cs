using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.Common.Survey
{
    public class ScheduleInfo
    {
        [JsonProperty(PropertyName = "lock")]
        internal string Lock { get; set; }

        [JsonProperty(PropertyName = "modules")]
        internal IList<ModuleInfo> Modules { get; set; }

        public void MergeScheduleInfo(ScheduleInfo externalScheduleInfo)
        {

        }
    }
}
