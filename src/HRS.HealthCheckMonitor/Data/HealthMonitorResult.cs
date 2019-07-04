using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace HRS.HealthCheckMonitor.Data
{
    /// <summary>
    /// A result of a health check
    /// </summary>
    public class HealthMonitorResult
    {
        private HealthMonitorStatus _monitorStatus = HealthMonitorStatus.NotChecked;

        /// <summary>
        /// The name given to the health check
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The status of the health check
        /// </summary>
        public HealthMonitorStatus Status
        {
            get => _monitorStatus;
            set
            {
                LastEvaluationDate = DateTime.UtcNow;
                if(_monitorStatus != value)
                {
                    CurrentStatusCount = 1;
                    _monitorStatus = value;
                }
                else
                {
                    CurrentStatusCount++;
                    CurrentStatusStarted = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// How many checks the result has been the current status
        /// </summary>
        public int CurrentStatusCount { get; set; } = 0;

        /// <summary>
        /// When the result first became the current status
        /// </summary>
        public DateTime CurrentStatusStarted { get; set; } = DateTime.MinValue;

        /// <summary>
        /// When the health check was last evaluated
        /// </summary>
        public DateTime LastEvaluationDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// How long the healh check took
        /// </summary>
        public TimeSpan EvaluationDuration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// The health check entries
        /// </summary>
        public IReadOnlyDictionary<string, HealthReportEntry> Entries { get; set; }
    }
}
