using HRS.HealthCheckMonitor.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace HRS.HealthCheckMonitor.Middleware
{
    internal class ApiEndpointMiddleware
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly HealthMonitorData _data;

        public ApiEndpointMiddleware(RequestDelegate _, HealthMonitorData data)
        {
            _data = data;
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() },
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.ContentType = Constants.DEFAULT_RESPONSE_CONTENT_TYPE;
            var responseContent = JsonConvert.SerializeObject(_data.MonitorResults, _serializerSettings);
            await context.Response.WriteAsync(responseContent);
        }
    }
}
