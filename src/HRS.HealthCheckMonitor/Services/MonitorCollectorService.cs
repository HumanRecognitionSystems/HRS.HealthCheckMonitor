using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Data;
using HRS.HostedServices.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HRS.HealthCheckMonitor.Services
{
    internal class MonitorCollectorService : HostedServices.HostedTimedService
    {
        private readonly HealthMonitorData _data;
        private readonly HealthMonitorCallbacks _callbacks;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;

        public MonitorCollectorService(HealthCheckMonitorOptions options, HealthMonitorData data, HealthMonitorCallbacks callbacks, IHttpClientFactory clientFactory, ILogger<MonitorCollectorService> logger) 
            : base(new HostedTimedOptions { InitialDelay = options.InitialEvaluationDelay, Interval = options.EvaluationInterval }, logger)
        {
            _data = data;
            _callbacks = callbacks;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var monitorTasks = new List<Task>();
            foreach (var checkSetting in _data.HealthChecks.Select(kv => kv.Value))
            {
                if(!checkSetting.Monitor)
                {
                    continue;
                }

                var result = _data.MonitorResults.GetOrAdd(checkSetting.Name, new HealthMonitorResult { Name = checkSetting.Name });
                var monitor = new MonitorHealthCheck(checkSetting, result, _clientFactory.CreateClient(Constants.HEALTHMONITOR_HTTP_CLIENT_NAME), _logger);
                monitorTasks.Add(monitor.Check(stoppingToken));
            }

            try
            {
                await Task.WhenAll(monitorTasks);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occured while trying to evaluate the health checks");
            }

            try
            {
                await _callbacks.ProcessCallbacks(_data);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occured while try to process the callbacks for the health checks");
            }
        }
    }
}
