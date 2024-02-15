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

using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.PowerShell.Common.Config;
using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.Common.Sanitizer
{
    public class AzurePSSanitizer
    {
        private readonly ISanitizerProviderResolver _providerResolver = DefaultProviderResolver.Instance;

        private readonly Stack<object> _sanitizingStack = new Stack<object>();

        private bool? _requireSecretsDetection;

        public bool RequireSecretsDetection
        {
            get
            {
                if (!_requireSecretsDetection.HasValue)
                {
                    try
                    {
                        AzureSession.Instance.TryGetComponent<IConfigManager>(nameof(IConfigManager), out var configManager);
                        _requireSecretsDetection = configManager?.GetConfigValue<bool>(ConfigKeysForCommon.ShowSecretsWarning);
                    }
                    catch
                    {
                        // Ignore exceptions
                    }

                    // Secrets detection is not enabled by default.
                    _requireSecretsDetection = _requireSecretsDetection ?? false;
                }

                return _requireSecretsDetection.Value;
            }
        }

        public void Sanitize(object sanitizingObject, SanitizerTelemetry telemetryData)
        {
            telemetryData.ShowSecretsWarning = true;

            if (sanitizingObject != null)
            {
                try
                {
                    var provider = _providerResolver.ResolveSanitizerProvider(sanitizingObject.GetType());
                    provider?.SanitizeValue(sanitizingObject, _sanitizingStack, _providerResolver, null, telemetryData);
                }
                catch (Exception ex)
                {
                    telemetryData.HasErrorInDetection = true;
                    telemetryData.DetectionError = ex;
                }
            }
        }
    }
}
