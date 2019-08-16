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

namespace Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
    using Microsoft.Rest.Azure;

    public static class ArgumentCompleterHelper
    {
        public static List<TItem> ReadAllPages<TItem>(Task<IPage<TItem>> task, Func<string, Task<IPage<TItem>>> nextTaskCreator)
        {
            var results = new List<TItem>();

            task.Wait();
            var page = task.Result;
            results.AddRange(page);

            while (!string.IsNullOrEmpty(page.NextPageLink))
            {
                task = nextTaskCreator(page.NextPageLink);
                task.Wait();
                page = task.Result;
                results.AddRange(page);
            }

            return results;
        }

        public static int HashContext(IAzureContext context)
        {
            return (context.Account.Id + context.Environment.Name + context.Subscription.Id + context.Tenant.Id).GetHashCode();
        }
    }
    public class CompleterScriptBuilder
    {
        private readonly string[] requiredParameters;
        private readonly string libNamespace;
        private readonly string className;
        private readonly string methodName;

        public CompleterScriptBuilder(string[] requiredParameters, string libNamespace, string className, string methodName)
        {
            this.requiredParameters = requiredParameters;
            this.libNamespace = libNamespace;
            this.className = className;
            this.methodName = methodName;
        }

        public override string ToString()
        {
            var parameters = new List<string>(requiredParameters);
            var parametersAssignments = string.Join(Environment.NewLine, parameters.Select((p, index) => $"$var{index} = $fakeBoundParameter['{p}']"));
            var parametersAsArguments = string.Join(", ", parameters.Select((_, index) => $"$var{index}"));
            return $@"param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
{parametersAssignments}
$candidates = [{libNamespace}.{className}]::{methodName}({parametersAsArguments})
$candidates | Where-Object {{ $_ -Like ""$wordToComplete*"" }} | Sort-Object | Get-Unique | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }}";
        }
    }
}
