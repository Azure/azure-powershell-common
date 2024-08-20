using Microsoft.Azure.Commands.Common.Authentication.Abstractions.Interfaces;

namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
{
    public class AuthenticationTelemetry: IAzureTelemetry <AuthTelemetryRecord> 
    {
        public AuthenticationTelemetryData GetTelemetryRecord(ICmdletContext cmdletContext)
        {
            var records = PopTelemetryRecord(cmdletContext);
            return records == null ? null : new AuthenticationTelemetryData(records);
        }
    }
}
