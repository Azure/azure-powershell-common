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

using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    /// <summary>
    /// A model class for a list of authenction telemetry records.
    /// </summary>
    public class AuthenticationTelemetryData
    {
        /// <summary>
        /// The first record of authentication telemetry data, usually describes the main method of this authentication process.
        /// </summary>
        public IAuthTelemetryRecord Head { get; } = null;

        /// <summary>
        /// The left part of authentication telemetry records.
        /// </summary>
        public IList<IAuthTelemetryRecord> Tail { get; } = new List<IAuthTelemetryRecord>();

        public AuthenticationTelemetryData(IEnumerable<IAuthTelemetryRecord> records)
        {
            var enumerator = records.GetEnumerator();
            if (enumerator.MoveNext())
            {
                Head = enumerator.Current;
            }

            while (enumerator.MoveNext())
            {
                Tail.Add(enumerator.Current);
            }
        }
    }
}
