using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Microsoft.Azure.Commands.ResourceManager.Common.Version2016_09_01.ArgumentCompleters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SkuNameCompleterAttribute : ArgumentCompleterAttribute
    {
        public SkuNameCompleterAttribute() : base(CreateScriptBlock())
        {
        }

        protected static ScriptBlock CreateScriptBlock()
        {
            string script = "param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)\n" +
                "$skuNames = [Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters.SkuCompleterAttribute]::GetSkuNames()\n" +
                "$skuNames | Where-Object { $_ -Like \"*$wordToComplete*\" } | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }";
            ScriptBlock scriptBlock = ScriptBlock.Create(script);
            return scriptBlock;
        }

        public static string[] GetSkuNames()
        {
            return new string[] { };
        }
    }
}
