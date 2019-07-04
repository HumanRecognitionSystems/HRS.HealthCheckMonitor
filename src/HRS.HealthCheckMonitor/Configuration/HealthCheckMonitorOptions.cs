using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace HRS.HealthCheckMonitor.Configuration
{
    /// <summary>
    /// The Health Check Monitor Options
    /// </summary>
    public class HealthCheckMonitorOptions
    {
        /// <summary>
        /// The configured list of health checks
        /// </summary>
        public List<HealthCheckSetting> HealthChecks { get; set; } = new List<HealthCheckSetting>();
        
        /// <summary>
        /// The interval between evaluating the health checks
        /// </summary>
        public TimeSpan EvaluationInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Initial Delay before the first evaluation of the health checks
        /// </summary>
        public TimeSpan InitialEvalutionDelay { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Timeout on the healthchecks response
        /// </summary>
        public TimeSpan HealthCheckTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// A directory that can contain additional health checks that may be added or removed from
        /// </summary>
        /// <remarks>
        /// This is mainly for dynamic services that may not be running all the time
        /// </remarks>
        public string HealthChecksDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The Api endpoint to use for returning result data
        /// </summary>
        /// <remarks>
        /// You must add <see cref="ApplicationBuilderExtensions.UseHealthCheckMonitorApi(IApplicationBuilder, Action{HealthCheckMonitorOptions})"/>
        /// to your Startup.Configure
        /// </remarks>
        public string ApiEndpoint { get; set; } = string.Empty;

    }
}
