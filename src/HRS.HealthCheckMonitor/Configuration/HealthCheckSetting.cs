namespace HRS.HealthCheckMonitor.Configuration
{
    /// <summary>
    /// Individual health check settings
    /// </summary>
    public class HealthCheckSetting
    {
        /// <summary>
        /// The name to give to a health check
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The uri of the health check
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Whether the health check should be monitored
        /// </summary>
        public bool Monitor { get; set; } = true;
    }
}