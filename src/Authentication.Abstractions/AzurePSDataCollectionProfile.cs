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

using Microsoft.Azure.PowerShell.Common.Config;
using System;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    public class AzurePSDataCollectionProfile
    {
        public const string EnvironmentVariableName = "Azure_PS_Data_Collection",
            OldDefaultFileName = "AzureDataCollectionProfile.json",
            DefaultFileName = "AzurePSDataCollectionProfile.json";

        private bool? _enableAzureDataCollection = null;

        public AzurePSDataCollectionProfile()
        {
        }

        [Obsolete("Data collection setting is supposed to be from Config API, " +
            "it should not be passed in the constructor. " +
            "Use AzurePSDataCollectionProfile() instead.")]
        public AzurePSDataCollectionProfile(bool enable)
        {
        }

        public bool? EnableAzureDataCollection {
            get
            {
                if (_enableAzureDataCollection.HasValue)
                {
                    return _enableAzureDataCollection.Value;
                }
                if (AzureSession.Instance.TryGetComponent<IConfigManager>(nameof(IConfigManager), out var configManager))
                {
                    _enableAzureDataCollection = configManager.GetConfigValue<bool>(ConfigKeysForCommon.EnableDataCollection);
                }
                return _enableAzureDataCollection;
            }
            set
            {
                throw new NotSupportedException("Setting data collection directly is not supported. Use Config API instead.");
            }
        }
    }
}
