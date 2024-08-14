using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    public class AuthenticationTelemetry: IAzureTelemetry <AuthTelemetryRecord> 
    {
        //public delegate string RequestIdAccquirer();

        //public static RequestIdAccquirer GetRequestId;

        //private static string requestId;

        public AuthenticationTelemetryData GetTelemetryRecord(ICmdletContext cmdletContext)
        {
            var records = PopTelemetryRecord(cmdletContext);
            return records == null ? null : new AuthenticationTelemetryData(records);
        }
    }
}
