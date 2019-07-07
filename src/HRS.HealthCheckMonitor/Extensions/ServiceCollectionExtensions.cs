using HRS.HealthCheckMonitor;
using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Data;
using HRS.HealthCheckMonitor.Services;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Adds health check monitor to an application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the health check monitor to an application
        /// </summary>
        /// <remarks>
        /// To make the health check monitor work at all add this to your starup ConfigureServices
        /// </remarks>
        public static IServiceCollection AddHealthCheckMonitor(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            var options = configuration.GetSection(Constants.HEALTHMONITOR_SECTION_SETTING_KEY).Get<HealthCheckMonitorOptions>();

            services.AddSingleton(options)
                .AddSingleton<HealthMonitorData>()
                .AddHostedService<MonitorCollectorService>()
                .AddSingleton<HealthMonitorCallbacks>();

            services.AddHttpClient(Constants.HEALTHMONITOR_HTTP_CLIENT_NAME, client => { client.Timeout = options.HealthCheckTimeout; });

            return services;
        }
    }
}
