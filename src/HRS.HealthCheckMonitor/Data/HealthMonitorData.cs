using HRS.HealthCheckMonitor.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HRS.HealthCheckMonitor.Data
{
    /// <summary>
    /// The data stored by the monitor
    /// </summary>
    public class HealthMonitorData
    {
        internal HealthMonitorData(HealthCheckMonitorOptions options)
        {
            AddUpdate(options.HealthChecks);
        }

        internal ConcurrentDictionary<string, HealthMonitorResult> MonitorResults { get; set; } = new ConcurrentDictionary<string, HealthMonitorResult>();
        internal ConcurrentDictionary<string, HealthCheckSetting> HealthChecks { get; set; } = new ConcurrentDictionary<string, HealthCheckSetting>();

        /// <summary>
        /// Add new health checks to the monitor
        /// </summary>
        /// <param name="settings">The settings for the healthchecks to add</param>
        public void AddUpdate(IEnumerable<HealthCheckSetting> settings)
        {
            foreach (var setting in settings)
            {
                HealthChecks.AddOrUpdate(setting.Name, setting, (k, v) => setting);
                MonitorResults.AddOrUpdate(setting.Name, new HealthMonitorResult { Name = setting.Name }, (k, v) =>
                 {
                     if (!setting.Monitor)
                     {
                         v.Status = HealthMonitorStatus.NotChecked;
                     }

                     return v;
                 });
            }
        }

        /// <summary>
        /// Remove health checks from the monitor
        /// </summary>
        /// <param name="settingNames">The names of the health checks to remove</param>
        public void Remove(IEnumerable<string> settingNames)
        {
            foreach (var settingName in settingNames)
            {
                HealthChecks.TryRemove(settingName, out var _);
                MonitorResults.TryRemove(settingName, out var _);
            }
        }

        /// <summary>
        /// return the names of all the registered health checks
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> HealthCheckNames()
        {
            return HealthChecks.Select(kv => kv.Key);
        }

        /// <summary>
        /// Return the details of a health check
        /// </summary>
        /// <param name="name">The name of the health check to get</param>
        public HealthCheckSetting HealthCheckSetting(string name)
        {
            if(HealthChecks.TryGetValue(name, out var setting))
            {
                return setting;
            }

            return default;
        }

        /// <summary>
        /// Get the results for a health check
        /// </summary>
        /// <param name="name">The name of the health check</param>
        public HealthMonitorResult Result(string name)
        {
            if(MonitorResults.TryGetValue(name, out var result))
            {
                return result;
            }

            return default;
        }
    }
}
