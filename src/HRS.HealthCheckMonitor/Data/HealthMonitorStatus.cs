namespace HRS.HealthCheckMonitor.Data
{
    /// <summary>
    /// The monitor status
    /// </summary>
    /// <remarks>
    /// Same as health check status with additional settings NotChecked, and Offline
    /// </remarks>
    public enum HealthMonitorStatus
    {
        /// <summary>
        /// The HealthCheck has not been evaluated yet
        /// </summary>
        NotChecked = 10,

        /// <summary>
        /// The HealthCheck is offline, usually failed to respond, or throw
        /// an exception and is unable to the monitor
        /// </summary>
        Offline = 11,

        /// <summary>
        /// The check returned Unhealthy
        /// </summary>
        Unhealthy = 0,

        /// <summary>
        /// The check returned Degraded
        /// </summary>
        Degraded = 1,

        /// <summary>
        /// The check returned Heathly
        /// </summary>
        Healthy = 2
    }
}
