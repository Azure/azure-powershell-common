using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.ResourceManager.Tests
{
    public class ArgumentCompleterUnitTests
    {
        [Fact]
        public void ReturnsCorrectScript()
        {
            var parameters = new[] { "Location", "Type" };
            var libNamespace = "Microsoft.Azure.Commands.Management.Compute.ArgumentCompleters";
            var className = "VmssSkuCompleterAttribute";
            var methodName = "GetSkuNames";
            string script = new ArgumentCompleterHelper.ScriptBuilder(parameters, libNamespace, className, methodName).ToString();
            Assert.Equal(@"param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
$var0 = $fakeBoundParameter['Location']
$var1 = $fakeBoundParameter['Type']
$candidates = [Microsoft.Azure.Commands.Management.Compute.ArgumentCompleters.VmssSkuCompleterAttribute]::GetSkuNames($var0, $var1)
$candidates | Where-Object { $_ -Like ""$wordToComplete*"" } | Sort-Object | Get-Unique | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }", script);
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
    }
}
