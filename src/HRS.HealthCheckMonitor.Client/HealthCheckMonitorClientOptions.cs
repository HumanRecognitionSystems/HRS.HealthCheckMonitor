using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HRS.HealthCheckMonitor.Client
{
    /// <summary>
    /// Used for providing the health check options preffered for the HealthCheckMonitor
    /// </summary>
    public static class HealthCheckMonitorClientOptions
    {
        /// <summary>
        /// Gives the healthcheckoptions set for the HealthCheckMonitor
        /// </summary>
        public static HealthCheckOptions HealthCheckOptions()
        {
            return new HealthCheckOptions
            {
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = 218,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                },
                ResponseWriter = HealthCheckMonitorWriter.HealthCheckMonitorResponse
            };
        }
    }
}
