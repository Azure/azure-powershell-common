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
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.Common.Sanitizer
{
    public class DetectedPropertiesInfo : IEnumerable<KeyValuePair<string, (HashSet<string> CrossCompanyCorrelatingIds, HashSet<string> Monikers)>>
    {
        private readonly Dictionary<string, (HashSet<string> CrossCompanyCorrelatingIds, HashSet<string> Monikers)> _internalProperties;

        public DetectedPropertiesInfo()
        {
            _internalProperties = new Dictionary<string, (HashSet<string> CrossCompanyCorrelatingIds, HashSet<string> Monikers)>();
        }

        public bool IsEmpty => _internalProperties.Count == 0;

        public IEnumerable<string> PropertyNames => _internalProperties.Keys;

        public void AddPropertyInfo(string propertyName, string crossCompanyCorrelatingId, string moniker)
        {
            if (!_internalProperties.TryGetValue(propertyName, out var propertyInfo))
            {
                propertyInfo = (new HashSet<string>(), new HashSet<string>());
                _internalProperties[propertyName] = propertyInfo;
            }

            propertyInfo.CrossCompanyCorrelatingIds.Add(crossCompanyCorrelatingId);
            propertyInfo.Monikers.Add(moniker);
        }

        public void AddPropertyInfo(string propertyName, HashSet<string> crossCompanyCorrelatingIds, HashSet<string> monikers)
        {
            if (!_internalProperties.TryGetValue(propertyName, out var propertyInfo))
            {
                propertyInfo = (new HashSet<string>(), new HashSet<string>());
                _internalProperties[propertyName] = propertyInfo;
            }

            propertyInfo.CrossCompanyCorrelatingIds.UnionWith(crossCompanyCorrelatingIds);
            propertyInfo.Monikers.UnionWith(monikers);
        }

        public bool ContainsProperty(string propertyName)
        {
            return _internalProperties.ContainsKey(propertyName);
        }

        public IEnumerator<KeyValuePair<string, (HashSet<string> CrossCompanyCorrelatingIds, HashSet<string> Monikers)>> GetEnumerator()
        {
            return _internalProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class SanitizerTelemetry
    {
        public bool ShowSecretsWarning { get; set; }

        public bool SecretsDetected { get; set; }

        public bool HasErrorInDetection { get; set; }

        public Exception DetectionError { get; set; }

        public TimeSpan SanitizeDuration { get; set; }

        public DetectedPropertiesInfo DetectedProperties { get; private set; }

        public SanitizerTelemetry(bool showSecretsWarning)
        {
            ShowSecretsWarning = showSecretsWarning;
            SecretsDetected = false;
            HasErrorInDetection = false;
            DetectedProperties = new DetectedPropertiesInfo();
        }

        public void Combine(SanitizerTelemetry telemetry)
        {
            if (telemetry != null)
            {
                ShowSecretsWarning = ShowSecretsWarning || telemetry.ShowSecretsWarning;
                SecretsDetected = SecretsDetected || telemetry.SecretsDetected;
                HasErrorInDetection = HasErrorInDetection || telemetry.HasErrorInDetection;
                DetectionError = DetectionError ?? telemetry.DetectionError;
                SanitizeDuration += telemetry.SanitizeDuration;
                foreach (var property in telemetry.DetectedProperties)
                {
                    DetectedProperties.AddPropertyInfo(property.Key, property.Value.CrossCompanyCorrelatingIds, property.Value.Monikers);
                }
            }
        }
    }
}
