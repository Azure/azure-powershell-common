﻿using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;

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

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// Represents a class for handling authentication telemetry.
    /// </summary>
    public class AuthenticationTelemetry : AzurePSCmdletConcurrentVault<AuthTelemetryRecord>
    {
        /// <summary>
        /// The name of the class.
        /// </summary>
        public const string Name = nameof(AuthenticationTelemetry);

        /// <summary>
        /// Gets the telemetry record for the specified cmdlet context.
        /// </summary>
        /// <param name="cmdletContext">The cmdlet context.</param>
        /// <returns>The authentication telemetry data.</returns>
        public AuthenticationTelemetryData GetTelemetryRecord(ICmdletContext cmdletContext)
        {
            var records = PopDataRecords(cmdletContext);
            return records == null ? null : new AuthenticationTelemetryData(records);
        }
    }
}
