using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HRS.HealthCheckMonitor.UnitTests.Core
{
    public class MockHttpClientFactory : IHttpClientFactory
    {
        private const string HTTP_REPOSNSE_TYPE = "application/json";

        public HttpClient CreateClient(string name)
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage message, CancellationToken _) =>
                {
                    if (message.RequestUri.PathAndQuery.EndsWith("/healthy", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(Report(HealthStatus.Healthy), Encoding.UTF8, HTTP_REPOSNSE_TYPE)
                        };
                    }
                    else if (message.RequestUri.PathAndQuery.EndsWith("/degraded", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = (HttpStatusCode)218,
                            Content = new StringContent(Report(HealthStatus.Degraded), Encoding.UTF8, HTTP_REPOSNSE_TYPE)
                        };
                    }
                    else if (message.RequestUri.PathAndQuery.EndsWith("/unhealthy", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.ServiceUnavailable,
                            Content = new StringContent(Report(HealthStatus.Unhealthy), Encoding.UTF8, HTTP_REPOSNSE_TYPE)
                        };
                    }
                    else
                    {
                        return null;
                    }
                })
                .Verifiable();

            return new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://test/")
            };
        }

        public static HttpClient Client(HttpStatusCode code, string content = null, string contentType = null)
        {
            HttpResponseMessage msg = new HttpResponseMessage(code);
            if (!string.IsNullOrWhiteSpace(content))
            {
                msg.Content = new StringContent(content, Encoding.UTF8, contentType);
            }

            if(code == HttpStatusCode.InternalServerError)
            {
                msg = null;
            }

            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(msg)
                .Verifiable();

            return new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://test/")
            };
        }

        public static string Report(HealthStatus status)
        {
            var entries = new Dictionary<string, HealthReportEntry>
            {
                {"Test", new HealthReportEntry(status, $"Standard Result:{status}", TimeSpan.FromMilliseconds(100), null, new Dictionary<string, object>()) }
            };
            return JsonConvert.SerializeObject(new HealthReport(entries, TimeSpan.FromMilliseconds(110)));
        }
    }
}
