using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
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
            string script = new ArgumentCompleterUtility.ScriptBuilder(parameters, libNamespace, className, methodName).ToString();
            Assert.Equal(@"param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
$var0 = $fakeBoundParameter['Location']
$var1 = $fakeBoundParameter['Type']
$skuNames = [Microsoft.Azure.Commands.Management.Compute.ArgumentCompleters.VmssSkuCompleterAttribute]::GetSkuNames($var0, $var1)
$locations | Where-Object { $_ -Like ""$wordToComplete*"" } | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }", script);
        }
    }
}
