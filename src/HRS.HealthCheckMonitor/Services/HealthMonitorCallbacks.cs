using HRS.HealthCheckMonitor.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRS.HealthCheckMonitor.Services
{
    /// <summary>
    /// Used to add or remove callbacks to the monitor
    /// </summary>
    public class HealthMonitorCallbacks
    {
        private readonly ILogger _logger;

        /// <summary>
        /// The consturctor for HealthMonitorCallbacks
        /// </summary>
        public HealthMonitorCallbacks(ILogger<HealthMonitorCallbacks> logger)
        {
            _logger = logger;
        }

        private readonly ConcurrentDictionary<string, Func<HealthMonitorResult, Task>> _callbacks = new ConcurrentDictionary<string, Func<HealthMonitorResult, Task>>();

        /// <summary>
        /// Add a callback for a health check
        /// </summary>
        /// <param name="healthCheckName">The name of the healthcheck</param>
        /// <param name="status">The status the health check should be in</param>
        /// <param name="callback">The callback</param>
        /// <remarks>If no health check exists with the name, no error is thrown. The callback is just never called</remarks>
        public void AddCallback(string healthCheckName, HealthMonitorStatus status, Func<HealthMonitorResult, Task> callback)
        {
            _callbacks.AddOrUpdate(CallbackName(healthCheckName, status), callback, (k, v) => callback);
        }

        /// <summary>
        /// Removes a callback for a healthcheck
        /// </summary>
        /// <param name="healthCheckName">The name of the healthcheck the callback is associated with</param>
        /// <param name="status">The status the callback is for</param>
        /// <remarks>If no health callback exists, no error is thrown</remarks>
        public void RemoveCallback(string healthCheckName, HealthMonitorStatus status)
        {
            _callbacks.TryRemove(CallbackName(healthCheckName, status), out var _);
        }

        internal async Task ProcessCallbacks(HealthMonitorData monitorData)
        {
            var results = monitorData.MonitorResults.Select(kv => kv.Value);
            var callbackTasks = new List<Task>();
            foreach (var result in results)
            {
                if(_callbacks.TryGetValue(CallbackName(result.Name, result.Status), out var callback))
                {
                    callbackTasks.Add(callback(result));
                }
            }
            
            try
            {
                await Task.WhenAll(callbackTasks);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "There was an error while processing the callbacks");
            }
        }

        private string CallbackName(string healthCheckName, HealthMonitorStatus status)
        {
            return $"{healthCheckName}:{status}";
        }
    }
}
