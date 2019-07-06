using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Data;
using HRS.HealthCheckMonitor.Services;
using HRS.HealthCheckMonitor.UnitTests.Core;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HRS.HealthCheckMonitor.UnitTests
{
    public class MonitorHealthCheckTests
    {
        private readonly XUnitTestLogger _logger;

        public MonitorHealthCheckTests(ITestOutputHelper outputHelper)
        {
            _logger = new XUnitTestLogger(outputHelper, "");
        }

        [Fact]
        public async Task ReturnHealthyCodeOnly()
        {
            var client = MockHttpClientFactory.Client(HttpStatusCode.OK);
            var result = new HealthMonitorResult();

            var healthCheck = new MonitorHealthCheck(new HealthCheckSetting { Uri = "test" }, result, client, _logger);
            await healthCheck.Check(default);

            Assert.Equal(HealthMonitorStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task ReturnDegradedCodeOnly()
        {
            var client = MockHttpClientFactory.Client((HttpStatusCode)218);
            var result = new HealthMonitorResult();

            var healthCheck = new MonitorHealthCheck(new HealthCheckSetting { Uri = "test" }, result, client, _logger);
            await healthCheck.Check(default);

            Assert.Equal(HealthMonitorStatus.Degraded, result.Status);
        }

        [Fact]
        public async Task ReturnUnhealthyCodeOnly()
        {
            var client = MockHttpClientFactory.Client(HttpStatusCode.ServiceUnavailable);
            var result = new HealthMonitorResult();

            var healthCheck = new MonitorHealthCheck(new HealthCheckSetting { Uri = "test" }, result, client, _logger);
            await healthCheck.Check(default);

            Assert.Equal(HealthMonitorStatus.Unhealthy, result.Status);
        }

        [Fact]
        public async Task ReturnHealthyStandardContent()
        {
            var client = MockHttpClientFactory.Client(HttpStatusCode.OK, "Healthy", "text/plain");
            var result = new HealthMonitorResult();

            var healthCheck = new MonitorHealthCheck(new HealthCheckSetting { Uri = "test" }, result, client, _logger);
            await healthCheck.Check(default);

            Assert.Equal(HealthMonitorStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task ReturnDegradedStandardContent()
        {
            var client = MockHttpClientFactory.Client(HttpStatusCode.OK, "Degraded", "text/plain");
            var result = new HealthMonitorResult();

            var healthCheck = new MonitorHealthCheck(new HealthCheckSetting { Uri = "test" }, result, client, _logger);
            await healthCheck.Check(default);

            Assert.Equal(HealthMonitorStatus.Degraded, result.Status);
        }

        [Fact]
        public async Task ReturnUnhealthyStandardContent()
        {
            var client = MockHttpClientFactory.Client(HttpStatusCode.OK, "Unhealthy", "text/plain");
            var result = new HealthMonitorResult();

            var healthCheck = new MonitorHealthCheck(new HealthCheckSetting { Uri = "test" }, result, client, _logger);
            await healthCheck.Check(default);

            Assert.Equal(HealthMonitorStatus.Unhealthy, result.Status);
        }

        [Fact]
        public async Task ReturnBadRequest()
        {
            var client = MockHttpClientFactory.Client(HttpStatusCode.InternalServerError);
            var result = new HealthMonitorResult();

            var healthCheck = new MonitorHealthCheck(new HealthCheckSetting { Uri = "test" }, result, client, _logger);
            await healthCheck.Check(default);

            Assert.Equal(HealthMonitorStatus.Offline, result.Status);
        }
    }
}
