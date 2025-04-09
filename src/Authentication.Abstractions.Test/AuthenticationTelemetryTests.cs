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
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Authentication.Abstractions.Test
{
    public class AuthenticationTelemetryTest
    {
        // Theory for PushTelemetryRecord tests
        [Theory]
        [InlineData(true, true, true, 1, 1, 0)]     // Valid context, valid record -> success
        [InlineData(false, true, false, 0, 0, 1)]   // Invalid context, valid record -> failure
        [InlineData(true, false, false, 0, 0, 1)]   // Valid context, null record -> failure
        [InlineData(null, true, false, 0, 0, 1)]    // Null context, valid record -> failure
        public void PushTelemetryRecord_Tests(bool? isContextValid, bool hasRecord, bool expectedResult,
                                             int expectedKeysCurrent, int expectedKeysAll, int expectedEmptyCount)
        {
            // Arrange
            var telemetry = new AuthenticationTelemetry();
            ICmdletContext context = null;
            AuthTelemetryRecord record = hasRecord ? new AuthTelemetryRecord() : null;

            if (isContextValid.HasValue)
            {
                var mockContext = new Mock<ICmdletContext>();
                mockContext.Setup(c => c.IsValid).Returns(isContextValid.Value);
                if (isContextValid.Value)
                {
                    mockContext.Setup(c => c.CmdletId).Returns("TestCmdlet");
                }
                context = mockContext.Object;
            }

            // Act
            var result = telemetry.PushDataRecord(context, record);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        // Test data for PopTelemetryRecord tests
        public static IEnumerable<object[]> PopTelemetryRecordTestData =>
            new List<object[]>
            {
                // Parameters: isContextValid, cmdletId, pushBeforePop, expectedNotNull, expectedKeysCount, expectedAllCount, expectedEmptyCount, expectedKeyNotFoundCount
                new object[] { true, "TestCmdlet", true, true, 0, 1, 0, 0 },    // Valid context with existing record
                new object[] { null, null, false, false, 0, 0, 1, 0 },           // Null context
                new object[] { false, null, false, false, 0, 0, 1, 0 },          // Invalid context
                new object[] { true, "TestCmdlet", false, false, 0, 0, 0, 1 }    // Valid context with non-existent key
            };

        [Theory]
        [MemberData(nameof(PopTelemetryRecordTestData))]
        public void PopTelemetryRecord_Tests(bool? isContextValid, string cmdletId, bool pushBeforePop,
                                           bool expectedNotNull, int expectedKeysCount, int expectedAllCount,
                                           int expectedEmptyCount, int expectedKeyNotFoundCount)
        {
            // Arrange
            var telemetry = new AuthenticationTelemetry();
            ICmdletContext context = null;

            if (isContextValid.HasValue)
            {
                var mockContext = new Mock<ICmdletContext>();
                mockContext.Setup(c => c.IsValid).Returns(isContextValid.Value);
                if (!string.IsNullOrEmpty(cmdletId))
                {
                    mockContext.Setup(c => c.CmdletId).Returns(cmdletId);
                }
                context = mockContext.Object;
            }

            // Push a record first if needed
            if (pushBeforePop && context != null)
            {
                var record = new AuthTelemetryRecord { TokenCredentialName = "TestCredential" };
                Assert.True(telemetry.PushDataRecord(context, record));
            }

            // Act
            var result = telemetry.PopDataRecords(context);

            // Assert
            Assert.Equal(expectedNotNull, result != null);
            if (expectedNotNull)
            {
                Assert.Single(result);
                Assert.Equal("TestCredential", result.FirstOrDefault()?.TokenCredentialName);
            }
        }

        // Test data for GetTelemetryRecord tests
        public static IEnumerable<object[]> GetTelemetryRecordTestData =>
            new List<object[]>
            {
        // Parameters: isContextValid, cmdletId, recordCount, expectedNotNull
        new object[] { true, "TestCmdlet", 1, true },     // Valid context with single record
        new object[] { true, "TestCmdlet", 3, true },     // Valid context with multiple records
        new object[] { null, null, 0, false },            // Null context
        new object[] { false, null, 0, false },           // Invalid context
        new object[] { true, "TestCmdlet", 0, false }     // Valid context with no records
            };

        [Theory]
        [MemberData(nameof(GetTelemetryRecordTestData))]
        public void GetTelemetryRecord_Tests(bool? isContextValid, string cmdletId, int recordCount,
                                           bool expectedNotNull)
        {
            // Arrange
            var telemetry = new AuthenticationTelemetry();
            ICmdletContext context = null;

            if (isContextValid.HasValue)
            {
                var mockContext = new Mock<ICmdletContext>();
                mockContext.Setup(c => c.IsValid).Returns(isContextValid.Value);
                if (!string.IsNullOrEmpty(cmdletId))
                {
                    mockContext.Setup(c => c.CmdletId).Returns(cmdletId);
                }
                context = mockContext.Object;
            }

            // Push records if needed
            for (int i = 0; i < recordCount; i++)
            {
                var record = new AuthTelemetryRecord { TokenCredentialName = $"TestCredential{i}" };
                Assert.True(telemetry.PushDataRecord(context, record));
            }

            // Act
            var result = telemetry.GetTelemetryRecord(context);

            // Assert
            Assert.Equal(expectedNotNull, result != null);

            if (expectedNotNull)
            {
                // Verify the AuthenticationTelemetryData contains our records
                Assert.NotNull(result.Primary);
                Assert.Equal("TestCredential0", result.Primary.TokenCredentialName);

                if (recordCount > 1)
                {
                    Assert.NotEmpty(result.Secondary);
                    Assert.Equal(recordCount - 1, result.Secondary.Count);

                    // Verify each record in the tail
                    for (int i = 1; i < recordCount; i++)
                    {
                        Assert.Equal($"TestCredential{i}", result.Secondary[i - 1].TokenCredentialName);
                    }
                }
                else
                {
                    Assert.Empty(result.Secondary);
                }
            }
        }
        [Fact]
        public void TelemetryRecord_ConcurrentTests()
        {
            // Arrange
            var telemetry = new AuthenticationTelemetry();
            const int pusherThreadCount = 10;

            // Create delegate to push records and get telemetry data
            Func<ICmdletContext, AuthenticationTelemetryData> PushAndGetFromMultipleThreads = (context) =>
            {
                // Create tasks for pushing records (10 threads)
                var pushTasks = new Task[pusherThreadCount];
                for (int t = 0; t < pusherThreadCount; t++)
                {
                    var threadId = t;
                    pushTasks[t] = Task.Run(() =>
                    {
                        // Each thread pushes one unique record
                        var record = new AuthTelemetryRecord { TokenCredentialName = $"TestCredential-{context.CmdletId}-{threadId}" };
                        Assert.True(telemetry.PushDataRecord(context, record));
                    });
                }
                Task.WaitAll(pushTasks); // Wait for all push tasks to complete
                return telemetry.GetTelemetryRecord(context);
            };

            // Create two contexts
            var mockContext1 = new Mock<ICmdletContext>();
            mockContext1.Setup(c => c.IsValid).Returns(true);
            mockContext1.Setup(c => c.CmdletId).Returns("TestCmdlet1");
            var context1 = mockContext1.Object;

            var mockContext2 = new Mock<ICmdletContext>();
            mockContext2.Setup(c => c.IsValid).Returns(true);
            mockContext2.Setup(c => c.CmdletId).Returns("TestCmdlet2");
            var context2 = mockContext2.Object;

            // Act
            // Run tasks in parallel
            var task1 = Task<AuthenticationTelemetryData>.Run(() => PushAndGetFromMultipleThreads(context1));
            var task2 = Task<AuthenticationTelemetryData>.Run(() => PushAndGetFromMultipleThreads(context2));

            // Wait for both tasks to complete
            Task.WaitAll(task1, task2);

            // Get results
            var results1 = task1.Result;
            var results2 = task2.Result;

            // Assert
            // Check that we have results from both contexts
            Assert.NotNull(results1);
            Assert.True(results1.Primary?.TokenCredentialName.StartsWith("TestCredential-TestCmdlet1"));
            Assert.Equal(9, results1.Secondary?.Count);
            Assert.NotNull(results2);
            Assert.True(results2.Primary?.TokenCredentialName.StartsWith("TestCredential-TestCmdlet2"));
            Assert.Equal(9, results2.Secondary?.Count);

            // Verify all records were retrieved (nothing left)
            Assert.Null(telemetry.GetTelemetryRecord(context1));
            Assert.Null(telemetry.GetTelemetryRecord(context2));
        }
    }
}