using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Rest.Azure;
using System.Collections;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.ResourceManager.Tests
{
    public class ArgumentCompleterUnitTests
    {
        [Fact]
        public void ReturnsCorrectScript()
        {
            // Given info about the cmdlet
            var parameters = new[] { "Location", "Type" };
            var libNamespace = "Microsoft.Azure.Commands.Management.Compute.ArgumentCompleters";
            var className = "VmssSkuCompleterAttribute";
            var methodName = "GetSkuNames";

            // When building the script
            string script = new ArgumentCompleterHelper.ScriptBuilder(parameters, libNamespace, className, methodName).ToString();

            // Example output:
            // Assert.Equal(@"param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
            // $var0 = $fakeBoundParameter['Location']
            // $var1 = $fakeBoundParameter['Type']
            // $candidates = [Microsoft.Azure.Commands.Management.Compute.ArgumentCompleters.VmssSkuCompleterAttribute]::GetSkuNames($var0, $var1)
            // $candidates | Where-Object { $_ -Like ""$wordToComplete*"" } | Sort-Object | Get-Unique | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }", script);

            // Then the output script
            Assert.True(script.IndexOf(parameters[0]) >= 0);
            Assert.True(script.IndexOf(parameters[0]) >= 0);
            Assert.True(script.IndexOf(libNamespace) >= 0);
            Assert.True(script.IndexOf(className) >= 0);
            Assert.True(script.IndexOf(methodName) >= 0);
        }

        [Fact]
        public void HashContext()
        {
            // Given context is null
            IAzureContext context = null;
            // When hashing the context
            int contextHash = ArgumentCompleterHelper.HashContext(context);
            // Then it shouldn't throw
        }

        [Fact]
        public void ReadPages()
        {
            // Given 2 pages
            IPage<int> page1 = new MockPage<int>("page1", new List<int> { 0});
            IPage<int> page2 = new MockPage<int>("page2", new List<int> { 1, 2 });

            (page1 as MockPage<int>).SetNextPage(page2 as MockPage<int>);

            var book = new Dictionary<string, IPage<int>>();
            book.Add("page1", page1);
            book.Add("page2", page2);

            // When calling ReadAllPages()
            var result = ArgumentCompleterHelper.ReadAllPages(Task.FromResult(page1), title => Task.FromResult(book[title]));

            // Then all pages are read, and their contents are combined
            Assert.Equal(new List<int> { 0, 1, 2 }, result);
        }

        private class MockPage<T> : IPage<T>
        {
            public MockPage(string title, List<T> values) {
                this.title = title;
                this.values = values;
            }

            public string title;

            public string NextPageLink => this._nextPageLink;

            private string _nextPageLink;

            public void SetNextPage(MockPage<T> page)
            {
                _nextPageLink = page.title;
            }

            public IEnumerator<T> GetEnumerator()
            {
                this.read = true;
                return values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private List<T> values = new List<T>();

            public bool read = false;
        }
    }
}
