using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Middleware;
using Microsoft.Extensions.DependencyInjection;
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
        public static IApplicationBuilder UseHealthCheckMonitorApi(this IApplicationBuilder app, Action<HealthCheckMonitorOptions> setup = null)
        {
            var opts = app.ApplicationServices.GetService<HealthCheckMonitorOptions>();
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
