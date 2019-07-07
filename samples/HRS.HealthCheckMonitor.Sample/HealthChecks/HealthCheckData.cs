using System.Collections.Concurrent;

namespace HRS.HealthCheckMonitor.Sample.HealthChecks
{
    public class HealthCheckData
    {
        public ConcurrentDictionary<string, HealthState> Checks { get; set; } = new ConcurrentDictionary<string, HealthState>();
    }
}
