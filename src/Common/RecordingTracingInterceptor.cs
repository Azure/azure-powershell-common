﻿// ----------------------------------------------------------------------------------
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

using Hyak.Common;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Extensions;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.ServiceManagement.Common.Models
{
    public class RecordingTracingInterceptor : Hyak.Common.ICloudTracingInterceptor
    {

        public RecordingTracingInterceptor(ConcurrentQueue<string> queue, IList<Regex> matchers = null, string clientRequestId = null)
        {
            MessageQueue = queue;
            Matchers = matchers;
            this.clientRequestId = clientRequestId;
        }

        public ConcurrentQueue<string> MessageQueue { get; private set; }

        private IList<Regex> Matchers { get; set; }

        private string clientRequestId;

        private void Write(string message, params object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                MessageQueue.CheckAndEnqueue(message);
            }
            else
            {
                MessageQueue.CheckAndEnqueue(string.Format(message, arguments));
            }
        }

        public void Information(string message)
        {
            MessageQueue.CheckAndEnqueue(message);
        }

        public void Configuration(string source, string name, string value)
        {
            // Ignore 
        }

        public void Enter(string invocationId, object instance, string method, IDictionary<string, object> parameters)
        {
            // Ignore    
        }

        public void SendRequest(string invocationId, HttpRequestMessage request)
        {
            // CmdletInfoHandler sets/updates x-ms-client-request-id during SendAsync() no matter if SDK sets x-ms-client-request-id.
            // Update request here to ensure its value consistent with real result.
            if (clientRequestId != null)
            {
                request.AddClientRequestId(clientRequestId);
            }
            Write(GeneralUtilities.GetLog(request, Matchers));
        }

        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            Write(GeneralUtilities.GetLog(response, Matchers));
        }

        public void Error(string invocationId, Exception ex)
        {
            // Ignore 
        }

        public void Exit(string invocationId, object result)
        {
            // Ignore 
        }

        public static void AddToContext(RecordingTracingInterceptor interceptor)
        {
            RemoveFromContext(interceptor);
            TracingAdapter.AddTracingInterceptor(interceptor);
        }

        public static void RemoveFromContext(RecordingTracingInterceptor interceptor)
        {
            try
            {
                TracingAdapter.RemoveTracingInterceptor(interceptor);
            }
            catch
            {
                // Ignore
            }
        }
    }
}

