using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Data;
using HRS.HealthCheckMonitor.Services;
using HRS.HealthCheckMonitor.UnitTests.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HRS.HealthCheckMonitor.UnitTests
{
    public class MonitorCollectorServiceTests
    {
        private readonly XUnitTestLogger<MonitorCollectorService> _collectorLogger;
        private readonly XUnitTestLogger<HealthMonitorCallbacks> _callbackLogger;
        private readonly MockHttpClientFactory _clientFactory;

        public MonitorCollectorServiceTests(ITestOutputHelper outputHelper)
        {
            _collectorLogger = new XUnitTestLogger<MonitorCollectorService>(outputHelper);
            _callbackLogger = new XUnitTestLogger<HealthMonitorCallbacks>(outputHelper);
            _clientFactory = new MockHttpClientFactory();
        }

        [Fact]
        public async Task AllHealthyResult()
        {
            var opts = Options();
            var data = await RunTest(opts);

            Assert.Equal(3, data.MonitorResults.Count);
            Assert.All(data.MonitorResults, r => Assert.True(r.Value.Status == HealthMonitorStatus.Healthy));
        }

        [Fact]
        public async Task OneResultDegraded()
        {
            var opts = Options();
            opts.HealthChecks[0].Uri = "degraded";
            var data = await RunTest(opts);

            Assert.Equal(3, data.MonitorResults.Count);
            Assert.Equal(HealthMonitorStatus.Degraded, data.MonitorResults["Check1"].Status);
            Assert.Equal(HealthMonitorStatus.Healthy, data.MonitorResults["Check2"].Status);
            Assert.Equal(HealthMonitorStatus.Healthy, data.MonitorResults["Check3"].Status);
        }

        [Fact]
        public async Task OneResultUnhealth()
        {
            var opts = Options();
            opts.HealthChecks[0].Uri = "unhealthy";
            var data = await RunTest(opts);

            Assert.Equal(3, data.MonitorResults.Count);
            Assert.Equal(HealthMonitorStatus.Unhealthy, data.MonitorResults["Check1"].Status);
            Assert.Equal(HealthMonitorStatus.Healthy, data.MonitorResults["Check2"].Status);
            Assert.Equal(HealthMonitorStatus.Healthy, data.MonitorResults["Check3"].Status);
        }

        [Fact]
        public async Task OneResultOffline()
        {
            var opts = Options();
            opts.HealthChecks[0].Uri = "offline";
            var data = await RunTest(opts);

            Assert.Equal(3, data.MonitorResults.Count);
            Assert.Equal(HealthMonitorStatus.Offline, data.MonitorResults["Check1"].Status);
            Assert.Equal(HealthMonitorStatus.Healthy, data.MonitorResults["Check2"].Status);
            Assert.Equal(HealthMonitorStatus.Healthy, data.MonitorResults["Check3"].Status);
        }

        [Fact]
        public async Task UnhealthyResultWithCallback()
        {
            var opts = Options();
            opts.HealthChecks[0].Uri = "unhealthy";

            var count = 0;
            var callbacks = new HealthMonitorCallbacks(_callbackLogger);
            callbacks.AddCallback("Check1", HealthMonitorStatus.Unhealthy, (result) => { count++; return Task.CompletedTask; });

            var data = await RunTest(opts, callbacks);

            Assert.Equal(3, data.MonitorResults.Count);
            Assert.Equal(HealthMonitorStatus.Unhealthy, data.MonitorResults["Check1"].Status);
            Assert.Equal(HealthMonitorStatus.Healthy, data.MonitorResults["Check2"].Status);
            Assert.Equal(HealthMonitorStatus.Healthy, data.MonitorResults["Check3"].Status);
            Assert.Equal(1, count);
        }

        private async Task<HealthMonitorData> RunTest(HealthCheckMonitorOptions options, HealthMonitorCallbacks callbacks = null)
        {
            var data = new HealthMonitorData(options);
            if(callbacks == null)
            {
                callbacks = new HealthMonitorCallbacks(_callbackLogger);
            }
            

            var service = new MonitorCollectorService(options, data, callbacks, _clientFactory, _collectorLogger);
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
            {
                await service.StartAsync(cts.Token);
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                await service.StopAsync(cts.Token);
            }

            return data;
        }

        private HealthCheckMonitorOptions Options()
        {
            return new HealthCheckMonitorOptions
            {
                InitialEvalutionDelay = TimeSpan.Zero,
                EvaluationInterval = TimeSpan.FromHours(5),
                HealthChecks = new List<HealthCheckSetting>
                {
                    new HealthCheckSetting{ Name = "Check1", Uri = "healthy" },
                    new HealthCheckSetting{ Name = "Check2", Uri = "healthy" },
                    new HealthCheckSetting{ Name = "Check3", Uri = "healthy" }
                }
            };
        }
    }
}
