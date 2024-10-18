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

using Microsoft.Azure.Commands.Common;

using System;
using System.Collections.Generic;

using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Test
{
    public class InvariantCultureUnitTests
    {
        [Theory]
        [MemberData(nameof(Data))]
        public void PrintDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
        {
            var expected = $"{month}/{day}/{year:0000} {(hour + 11) % 12 + 1}:{minute:00}:{second:00}";
            expected += hour < 12 ? " AM" : " PM";

            var dateTime = new DateTime(year, month, day, hour, minute, second, millisecond, kind);
            Assert.Equal(expected, dateTime.ToInvariantString());
        }

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new object[] { 1, 1, 1, 12, 0, 0, 0, DateTimeKind.Local},
            new object[] { 1, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc},
            new object[] { 2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Local},
            new object[] { 2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc},
            new object[] { 2000, 12, 31, 23, 59, 59, 0, DateTimeKind.Local},
            new object[] { 2000, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc},
            new object[] { 1970, 10, 18, 3, 3, 3, 0, DateTimeKind.Local},
            new object[] { 1970, 10, 18, 3, 3, 3, 0, DateTimeKind.Utc},
            new object[] { 9999, 1, 18, 22, 30, 17, 0, DateTimeKind.Local},
            new object[] { 9999, 1, 18, 22, 30, 17, 0, DateTimeKind.Utc},
        };
    }
}
