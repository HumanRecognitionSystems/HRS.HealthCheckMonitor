using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Middleware;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extensions for Health Check Monitor
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the Health check monitor api to your application
        /// </summary>
        /// <remarks>
        /// This provides an easy and direct way to access the results as a json object in your javascript
        /// </remarks>
        private static IApplicationBuilder UseHealthCheckMonitorApi(IApplicationBuilder app, Action<HealthCheckMonitorOptions> setup = null)
        {
            var opts = (HealthCheckMonitorOptions)app.ApplicationServices.GetService(typeof(HealthCheckMonitorOptions));
            setup?.Invoke(opts);
            if(string.IsNullOrWhiteSpace(opts.ApiEndpoint) || !opts.ApiEndpoint.StartsWith("/"))
            {
                throw new ArgumentException("The Api Uri can't be empty and needs to start with a '/'");
            }

            app.Map(opts.ApiEndpoint, builder => builder.UseMiddleware<ApiEndpointMiddleware>());
            return app;
        }
    }
}
