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

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.Common.Sanitizer
{
    public class SanitizerTelemetry
    {
        public bool ShowSecretsWarning { get; set; } = false;

        public HashSet<string> DetectedProperties { get; set; } = new HashSet<string>();

        public bool HasErrorInDetection { get; set; } = false;

        public Exception DetectionError { get; set; }

        public TimeSpan SanitizeDuration { get; set; }
    }
}
