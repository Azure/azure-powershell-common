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

using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Concurrent;

namespace Microsoft.WindowsAzure.Commands.Common
{
    public class HttpTracingPolicy : HttpPipelineSynchronousPolicy, ICloneable
    {
        public ConcurrentQueue<string> MessageQueue { get; private set; }

        public HttpTracingPolicy(ConcurrentQueue<string> queue)
        {
            MessageQueue = queue;
        }

        public override void OnSendingRequest(HttpMessage message)
        {
            MessageQueue.CheckAndEnqueue(GeneralUtilities.GetLog(message.Request));
        }

        public override void OnReceivedResponse(HttpMessage message)
        {
            MessageQueue.CheckAndEnqueue(GeneralUtilities.GetLog(message.Response));
        }

        public object Clone()
        {
            return new HttpTracingPolicy(MessageQueue);
        }
    }
}
