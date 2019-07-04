using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Threading.Tasks;

namespace HRS.HealthCheckMonitor.Client
{
    /// <summary>
    /// Provides the health check reponse writer set up for full json
    /// </summary>
    public static class HealthCheckMonitorWriter
    {
        const string DEFAULT_CONTENT_TYPE = "application/json";
        private static Lazy<JsonSerializerSettings> _settings = new Lazy<JsonSerializerSettings>(() =>
        {
            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new [] { new StringEnumConverter() },
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
            return settings;
        });

        /// <summary>
        /// Writes the Health Report to the response
        /// </summary>
        /// <param name="httpContext">The context of the reponse</param>
        /// <param name="report">The report to write to the response</param>
        /// <returns></returns>
        public static async Task HealthCheckMonitorResponse(HttpContext httpContext, HealthReport report)
        {
            httpContext.Response.ContentType = DEFAULT_CONTENT_TYPE;
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(report, _settings.Value));
        }
    }
}
