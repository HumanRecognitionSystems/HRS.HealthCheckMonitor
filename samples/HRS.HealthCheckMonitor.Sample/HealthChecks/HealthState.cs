using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HRS.HealthCheckMonitor.Sample.HealthChecks
{
    public class HealthState
    {
        public HealthStatus Status { get; set; }
        public string Description { get; set; }
    }
}
