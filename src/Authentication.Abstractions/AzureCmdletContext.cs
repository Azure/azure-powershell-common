using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    public class AzureCmdletContext : ICmdletContext
    {
        private string cmdletId;

        public const ICmdletContext CmdletNone = null;

        public AzureCmdletContext(string id)
        {
            cmdletId = id;
        }
        public string CmdletId
        { 
            get => cmdletId;
            set => cmdletId = value;
        }

        public bool IsValid
        { 
            get => !string.IsNullOrEmpty(cmdletId);
        }
    }
}
