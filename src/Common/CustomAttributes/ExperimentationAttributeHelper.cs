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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.WindowsAzure.Commands.Common.CustomAttributes
{
    public class ExperimentationAttributeHelper
    {
        public const string SUPPRESS_ERROR_OR_WARNING_MESSAGE_ENV_VARIABLE_NAME = "SuppressAzurePowerShellBreakingChangeWarnings";

        /// <summary>
        /// Process CmdletExperimentation attribute in runtime
        /// </summary>
        /// <param name="type"></param>
        /// <param name="invocationInfo"></param>
        /// <param name="writeOutput"></param>
        public static void ProcessCustomAttributesAtRuntime(Type type, InvocationInfo invocationInfo, Action<string> writeOutput)
        {
            bool supressWarningOrError = false;

            try
            {
                supressWarningOrError = bool.Parse(System.Environment.GetEnvironmentVariable(SUPPRESS_ERROR_OR_WARNING_MESSAGE_ENV_VARIABLE_NAME));
            }
            catch (Exception)
            {
                //no action
            }

            if (supressWarningOrError)
            {
                //Do not process the attributes at runtime... The env variable to override the warning messages is set
                return;
            }

            List<CmdletExperimentationAttribute> attributes = new List<CmdletExperimentationAttribute>(GetAllAttributesInType(type, invocationInfo));

            if (attributes != null && attributes.Count > 0)
            {
                foreach (CmdletExperimentationAttribute attribute in attributes)
                {
                    attribute.PrintCustomAttributeInfo(writeOutput);
                }
            }
        }

        private static IEnumerable<CmdletExperimentationAttribute> GetAllAttributesInType(Type type, InvocationInfo invocationInfo)
        {
            List<CmdletExperimentationAttribute> attributeList = new List<CmdletExperimentationAttribute>();

            attributeList.AddRange(type.GetCustomAttributes(typeof(CmdletExperimentationAttribute), false).Cast<CmdletExperimentationAttribute>());

            foreach (MethodInfo m in type.GetRuntimeMethods())
            {
                attributeList.AddRange((m.GetCustomAttributes(typeof(CmdletExperimentationAttribute), false).Cast<CmdletExperimentationAttribute>()));
            }

            foreach (FieldInfo f in type.GetRuntimeFields())
            {
                attributeList.AddRange(f.GetCustomAttributes(typeof(CmdletExperimentationAttribute), false).Cast<CmdletExperimentationAttribute>());
            }

            foreach (PropertyInfo p in type.GetRuntimeProperties())
            {
                attributeList.AddRange(p.GetCustomAttributes(typeof(CmdletExperimentationAttribute), false).Cast<CmdletExperimentationAttribute>());
            }

            return invocationInfo == null ? attributeList : attributeList.Where(e => e.IsApplicableToInvocation(invocationInfo));
        }
    }
}
