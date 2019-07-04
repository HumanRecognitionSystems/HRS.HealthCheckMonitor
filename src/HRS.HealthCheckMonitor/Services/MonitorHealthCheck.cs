using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Data;
using HRS.HealthCheckMonitor.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HRS.HealthCheckMonitor.Services
{
    internal class MonitorHealthCheck
    {
        private readonly HealthCheckSetting _setting;
        private readonly HealthMonitorResult _result;
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public MonitorHealthCheck(HealthCheckSetting setting, HealthMonitorResult result, HttpClient client, ILogger logger)
        {
            _setting = setting;
            _result = result;
            _client = client;
            _logger = logger;
        }

        public async Task Check(CancellationToken token)
        {
            try
            {
                var response = await _client.GetAsync(_setting.Uri, token);
                if(token.IsCancellationRequested)
                {
                    return;
                }

                if(response.IsSuccessStatusCode)
                {
                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        _result.Status = HealthMonitorStatus.Healthy;
                    }
                    if((int)response.StatusCode == Constants.HEALTHMONITOR_DEGRADED)
                    {
                        _result.Status = HealthMonitorStatus.Degraded;
                    }
                }
                else
                {
                    _result.Status = HealthMonitorStatus.Unhealthy;
                }

                await CheckContent(response.Content);
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, $"An error occured while contacting the \"{_setting.Name}\" health check at endpoint {_setting.Uri}");
                _result.Status = HealthMonitorStatus.Offline;
                _result.Entries = null;
            }
        }

        private async Task CheckContent(HttpContent content)
        {
            if(content.Headers.ContentType.MediaType == Constants.DEFAULT_RESPONSE_CONTENT_TYPE)
            {
                try
                {
                    var report = await content.ReadAsJsonAsync<HealthReport>();
                    _result.Status = (HealthMonitorStatus)report.Status;
                    _result.EvaluationDuration = report.TotalDuration;
                    _result.Entries = report.Entries;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Error while reading the json health report from \"{_setting.Name}\" health check at endpoint {_setting.Uri}");
                    _result.Status = HealthMonitorStatus.Unhealthy;
                }
            }
            else if(content.Headers.ContentType.MediaType == "text/plain")
            {
                var stringContent = await content.ReadAsStringAsync();
                switch(stringContent)
                {
                    case "Healthy": _result.Status = HealthMonitorStatus.Healthy; break;
                    case "Degraded": _result.Status = HealthMonitorStatus.Degraded; break;
                    default: _result.Status = HealthMonitorStatus.Unhealthy; break;
                }
            }
        }
    }
}
