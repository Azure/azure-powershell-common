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
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Owns the change safety dynamic parameter definitions (-AcquirePolicyToken and -ChangeReference)
    /// and their name constants, so they can be shared across cmdlet base classes and pipelines.
    /// </summary>
    public static class ChangeSafetyParameters
    {
        public const string AcquirePolicyTokenParamName = "AcquirePolicyToken";
        public const string ChangeReferenceParamName = "ChangeReference";

        /// <summary>
        /// Adds the change safety dynamic parameters (-AcquirePolicyToken and -ChangeReference) to the
        /// supplied dictionary if they are not already present.
        /// </summary>
        /// <param name="dict">The runtime defined parameter dictionary to add the parameters to.</param>
        public static void AddChangeSafetyParameters(RuntimeDefinedParameterDictionary dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (!dict.ContainsKey(AcquirePolicyTokenParamName))
            {
                dict.Add(AcquirePolicyTokenParamName, new RuntimeDefinedParameter(
                    AcquirePolicyTokenParamName,
                    typeof(SwitchParameter),
                    new Collection<Attribute>
                    {
                        new ParameterAttribute
                        {
                            HelpMessage = "Acquire an Azure Policy token automatically for this resource operation.",
                            ParameterSetName = ParameterAttribute.AllParameterSets
                        }
                    }));
            }

            if (!dict.ContainsKey(ChangeReferenceParamName))
            {
                dict.Add(ChangeReferenceParamName, new RuntimeDefinedParameter(
                    ChangeReferenceParamName,
                    typeof(string),
                    new Collection<Attribute>
                    {
                        new ParameterAttribute
                        {
                            HelpMessage = "The change reference resource ID for this resource operation.",
                            ParameterSetName = ParameterAttribute.AllParameterSets
                        }
                    }));
            }
        }
    }
}
