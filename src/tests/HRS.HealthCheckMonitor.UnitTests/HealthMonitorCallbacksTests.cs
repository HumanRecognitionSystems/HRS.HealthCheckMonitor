using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Data;
using HRS.HealthCheckMonitor.Services;
using HRS.HealthCheckMonitor.UnitTests.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HRS.HealthCheckMonitor.UnitTests
{
    public class HealthMonitorCallbacksTests
    {
        private readonly XUnitTestLogger<HealthMonitorCallbacks> _logger;

        public HealthMonitorCallbacksTests(ITestOutputHelper outputHelper)
        {
            _logger = new XUnitTestLogger<HealthMonitorCallbacks>(outputHelper);
        }

        [Fact]
        public async Task TestCallbacksAll()
        {
            var callbacks = new HealthMonitorCallbacks(_logger);
            var healthyCount = 0;
            var degradedCount = 0;
            var unhealthyCount = 0;
            
            callbacks.AddCallback("Check1", HealthMonitorStatus.Healthy, (result) => { healthyCount++; return Task.CompletedTask; });
            callbacks.AddCallback("Check2", HealthMonitorStatus.Degraded, (result) => { degradedCount++; return Task.CompletedTask; });
            callbacks.AddCallback("Check3", HealthMonitorStatus.Unhealthy, (result) => { unhealthyCount++; return Task.CompletedTask; });

            await callbacks.ProcessCallbacks(Data());

            Assert.Equal(1, healthyCount);
            Assert.Equal(1, degradedCount);
            Assert.Equal(1, unhealthyCount);
        }

        [Fact]
        public async Task TestCallbacksTwice()
        {
            var callbacks = new HealthMonitorCallbacks(_logger);
            var healthyCount = 0;
            var degradedCount = 0;
            var unhealthyCount = 0;

            callbacks.AddCallback("Check1", HealthMonitorStatus.Healthy, (result) => { healthyCount++; return Task.CompletedTask; });
            callbacks.AddCallback("Check2", HealthMonitorStatus.Degraded, (result) => { degradedCount++; return Task.CompletedTask; });
            callbacks.AddCallback("Check3", HealthMonitorStatus.Unhealthy, (result) => { unhealthyCount++; return Task.CompletedTask; });

            await callbacks.ProcessCallbacks(Data());
            await callbacks.ProcessCallbacks(Data());

            Assert.Equal(2, healthyCount);
            Assert.Equal(2, degradedCount);
            Assert.Equal(2, unhealthyCount);
        }

        [Fact]
        public async Task TestCallbacksRemoval()
        {
            var callbacks = new HealthMonitorCallbacks(_logger);
            var healthyCount = 0;
            var degradedCount = 0;
            var unhealthyCount = 0;

            callbacks.AddCallback("Check1", HealthMonitorStatus.Healthy, (result) => { healthyCount++; return Task.CompletedTask; });
            callbacks.AddCallback("Check2", HealthMonitorStatus.Degraded, (result) => { degradedCount++; return Task.CompletedTask; });
            callbacks.AddCallback("Check3", HealthMonitorStatus.Unhealthy, (result) => { unhealthyCount++; return Task.CompletedTask; });

            await callbacks.ProcessCallbacks(Data());

            callbacks.RemoveCallback("Check1", HealthMonitorStatus.Healthy);

            await callbacks.ProcessCallbacks(Data());

            Assert.Equal(1, healthyCount);
            Assert.Equal(2, degradedCount);
            Assert.Equal(2, unhealthyCount);
        }

        [Fact]
        public async Task TestCallbacksAdd()
        {
            var callbacks = new HealthMonitorCallbacks(_logger);
            var healthyCount = 0;
            var degradedCount = 0;
            var unhealthyCount = 0;

            callbacks.AddCallback("Check1", HealthMonitorStatus.Healthy, (result) => { healthyCount++; return Task.CompletedTask; });
            
            await callbacks.ProcessCallbacks(Data());

            callbacks.AddCallback("Check2", HealthMonitorStatus.Degraded, (result) => { degradedCount++; return Task.CompletedTask; });

            await callbacks.ProcessCallbacks(Data());

            Assert.Equal(2, healthyCount);
            Assert.Equal(1, degradedCount);
            Assert.Equal(0, unhealthyCount);
        }



        private HealthMonitorData Data()
        {
            var data = new HealthMonitorData(Options());
            data.MonitorResults["Check1"].Status = HealthMonitorStatus.Healthy;
            data.MonitorResults["Check2"].Status = HealthMonitorStatus.Degraded;
            data.MonitorResults["Check3"].Status = HealthMonitorStatus.Unhealthy;

            return data;
        }

        private HealthCheckMonitorOptions Options()
        {
            return new HealthCheckMonitorOptions
            {
                InitialEvaluationDelay = TimeSpan.Zero,
                EvaluationInterval = TimeSpan.FromHours(5),
                HealthChecks = new List<HealthCheckSetting>
                {
                    new HealthCheckSetting{ Name = "Check1", Uri = "healthy" },
                    new HealthCheckSetting{ Name = "Check2", Uri = "unhealthy" },
                    new HealthCheckSetting{ Name = "Check3", Uri = "degraded" }
                }
            };
        }
    }
}
