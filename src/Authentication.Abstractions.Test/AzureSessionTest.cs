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
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Authentication.Abstractions.Test
{
    public class AzureSessionTest : IDisposable

    {
        // Concrete implementation of AzureSession for testing
        private class TestAzureSession : AzureSession
        {
            public override TraceLevel AuthenticationLegacyTraceLevel { get; set; }
            public override TraceListenerCollection AuthenticationTraceListeners => Trace.Listeners;
            public override SourceLevels AuthenticationTraceSourceLevel { get; set; }
        }

        private IAzureSession oldSession = null;

        public AzureSessionTest()
        {
            try
            {
                IAzureSession oldSession = AzureSession.Instance;

            }
            catch (Exception)
            {
            }
            AzureSession.Initialize(() => new TestAzureSession(), true);
        }
        
        public void Dispose()
        {
            // Assign AzureSession.Instance back to oldSession
            AzureSession.Initialize(() => oldSession, true);
        }

        private class TestComponent
        {
            public string Name { get; set; }
            private int id;
            private ConcurrentQueue<int> clist = null;
            public int Size
            {
                get => clist.Count();
            }

            public int Id
            {
                get => id;
            }

            public TestComponent(string name, int id)
            {
                Name = name;
                clist = new ConcurrentQueue<int>();
                this.id = id;
            }

            public void Append(int i)
            {
                clist.Enqueue(i);
            }
        }

        private Object lockObject = new Object();

        // Function to register and retrieve component
        private Dictionary<string, int> RegisterAndRetrieveComponent(string componentName, int componentValue, bool overwritten)
        {
            AzureSession.Instance.RegisterComponent(componentName, () => new TestComponent(componentName, componentValue), overwritten);
            AzureSession.Instance.TryGetComponent(componentName, out TestComponent component);
            lock (lockObject)
            {
                component.Append(1);
                return new Dictionary<string, int>
                {
                    { "id", component.Id },
                    { "size", component.Size }
                };
            }
        }

        [Fact]
        public void TestClearComponents()
        {
            string testComponent1 = "TestComponent1";
            string testComponent2 = "TestComponent2";

            // Register components
            AzureSession.Instance.RegisterComponent(testComponent1, () => "Value1");
            AzureSession.Instance.RegisterComponent(testComponent2, () => "Value2");

            // Clear all components
            AzureSession.Instance.ClearComponents();

            // Verify they are gone
            Assert.False(AzureSession.Instance.TryGetComponent(testComponent1, out string _));
            Assert.False(AzureSession.Instance.TryGetComponent(testComponent2, out string _));
        }

        [Fact]
        public void TestComponentRegistrationDifferentComponentNoOverwritten()
        {
            string testComponent = "TestComponent";

            var tasks = new List<Task<Dictionary<string, int>>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(new Task<Dictionary<string,int>>(
                    (object state) =>
                    {
                        int i = (int)state;
                        return RegisterAndRetrieveComponent($"{testComponent}{i}", i, false);
                    },
                    i));
            }

            foreach(var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());

            // Verify the results
            for (int i = 0; i < 10; i++)
            {
                var result = tasks[i].Result;
                Assert.Equal(1, result["size"]);
            }
            AzureSession.Instance.ClearComponents();
        }

        [Fact]
        public void TestComponentRegistrationDifferentComponentOverwritten()
        {
            string testComponent = "TestComponent";

            var tasks = new List<Task<Dictionary<string, int>>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(new Task<Dictionary<string, int>>(
                    (object state) =>
                    {
                        int i = (int)state;
                        return RegisterAndRetrieveComponent($"{testComponent}{i}", i, true);
                    },
                    i));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());

            // Verify the results
            for (int i = 0; i < 10; i++)
            {
                var result = tasks[i].Result;
                Assert.Equal(1, result["size"]);
            }
            AzureSession.Instance.ClearComponents();
        }

        [Fact]
        public void TestComponentRegistrationSameComponentNoOverwritten()
        {
            string testComponent = "TestComponent";

            // Create 10 tasks to run the function in parallel
            var tasks = new List<Task<Dictionary<string, int>>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(new Task<Dictionary<string, int>>(
                    (object state) =>
                    {
                        int i = (int)state;
                        return RegisterAndRetrieveComponent(testComponent, i, false);
                    },
                    i));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());

            // Verify the results
            var results = new int[10];

            Assert.Single(tasks.Select(t => t.Result["id"]).Distinct());
            var checkList = tasks.Select(t => t.Result["size"]);
            Assert.Equal(10, checkList.Distinct().Count());
            Assert.Equal(10, checkList.Max());
            Assert.Equal(1, checkList.Min());
            AzureSession.Instance.ClearComponents();
        }

        [Fact]
        public void TestComponentRegistrationSameComponentOverwritten()
        {
            string testComponent = "TestComponent";

            // Create 10 tasks to run the function in parallel
            var tasks = new List<Task<Dictionary<string, int>>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(new Task<Dictionary<string, int>>(
                    (object state) =>
                    {
                        int i = (int)state;
                        return RegisterAndRetrieveComponent(testComponent, i, true);
                    },
                    i));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());

            // Verify the results
            AzureSession.Instance.TryGetComponent(testComponent, out TestComponent component);
            Assert.Equal(1, component.Size);
            void CheckResults(List<Task<Dictionary<string, int>>> tasks, int id)
            {
                var checkList = tasks.Where(t => t.Result["id"] == id);
                var count = checkList.Count();
                Assert.Equal(count, checkList.Distinct().Count());
                if (count > 0)
                {
                    Assert.Equal(count, checkList?.Select(t => t.Result["size"])?.Max());
                }
            }
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"id={i}");
                CheckResults(tasks, i);
            }
            AzureSession.Instance.ClearComponents();
        }

        [Fact]
        public void TestComponentRegistrationAndUnregistrationInDifferentThreads()
        {
            string[] testComponents = { "TestComponent1", "TestComponent2", "TestComponent3" };
            string componentValue = "TestValue";

            Func<string, string, string> RegisterAndUnregisterComponent = (testComponent, componentValue) =>
            {
                var taskRegister = Task.Run(() => AzureSession.Instance.RegisterComponent(testComponent, () => componentValue));
                taskRegister.Wait();

                Assert.True(AzureSession.Instance.TryGetComponent(testComponent, out string retrievedValue));
                Assert.Equal(componentValue, retrievedValue);

                var unregisterTask = Task.Run(() => AzureSession.Instance.UnregisterComponent<string>(testComponent));
                unregisterTask.Wait();
                return retrievedValue;
            };

            // Register components in parallel
            var tasks = new ConcurrentBag<Task>();
            foreach (var component in testComponents)
            {
                Task.Run(() => RegisterAndUnregisterComponent(component, componentValue)).ContinueWith(t => tasks.Add(t), TaskScheduler.Default);
            }

            // Wait for all register tasks to complete
            Task.WaitAll(tasks.ToArray());

            // Verify components are unregistered
            foreach (var component in testComponents)
            {
                Assert.False(AzureSession.Instance.TryGetComponent(component, out string _));
            }
        }

        [Fact]
        public void TestEventHandler()
        {
            bool eventRaised = false;
            var listener = new TestSessionListener(() => eventRaised = true);

            AzureSession.Instance.RegisterComponent("listener", () => listener);
            AzureSession.Instance.RaiseContextClearedEvent();
            Assert.True(eventRaised);

            eventRaised = false;
            AzureSession.Instance.UnregisterComponent<TestSessionListener>("listener");
            AzureSession.Instance.RaiseContextClearedEvent();
            Assert.False(eventRaised);
        }

        private class TestSessionListener : IAzureSessionListener
        {
            private Action _callback;

            public TestSessionListener(Action callback)
            {
                _callback = callback;
            }

            public void OnEvent(object sender, AzureSessionEventArgs e)
            {
                if (e.Type == AzureSessionEventType.ContextCleared)
                {
                    _callback();
                }
            }
        }
    }
}
