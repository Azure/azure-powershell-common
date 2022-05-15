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
    public abstract class DataCollectionController
    {
        public const string RegistryKey = "DataCollectionController";
        public abstract AzurePSDataCollectionProfile GetProfile(Action warningWriter);

        static AzurePSDataCollectionProfile Initialize(IAzureSession session)
        {
            return new AzurePSDataCollectionProfile();
        }

        [Obsolete("Use config API to update data collection settings.")]
        public static void WritePSDataCollectionProfile(IAzureSession session, AzurePSDataCollectionProfile profile)
        {
        }

        public static DataCollectionController Create(IAzureSession session)
        {
            return new MemoryDataCollectionController(Initialize(session));
        }

        public static DataCollectionController Create(AzurePSDataCollectionProfile profile)
        {
            return new MemoryDataCollectionController(profile);
        }

        class MemoryDataCollectionController : DataCollectionController
        {
            public MemoryDataCollectionController()
            {
            }

            public MemoryDataCollectionController(AzurePSDataCollectionProfile enabled)
            {
            }

            public override AzurePSDataCollectionProfile GetProfile(Action warningWriter)
            {
                return new AzurePSDataCollectionProfile();
            }
        }
    }
}
