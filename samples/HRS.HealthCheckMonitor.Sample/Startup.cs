using HRS.HealthCheckMonitor.Sample.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HRS.HealthCheckMonitor.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //**Client Side: add Health Checks
            AddHealthChecks(services);

            //**Monitor Side: add the health check monitor
            services.AddHealthCheckMonitor();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //**Client Side: use the health checks with the Monitor Health Client Library
            app.UseHealthChecks("/health", Client.HealthCheckMonitorClientOptions.HealthCheckOptions());

            //**Monitor Side: add the healthcheck monitor Api
            app.UseHealthCheckMonitorApi();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseMvc();
        }

        private void AddHealthChecks(IServiceCollection services)
        {
            var hcd = new HealthCheckData();

            hcd.Checks.TryAdd("Health1", new HealthState { Status = HealthStatus.Healthy, Description = "Health Check One" });
            hcd.Checks.TryAdd("Health2", new HealthState { Status = HealthStatus.Healthy, Description = "Health Check Two" });
            hcd.Checks.TryAdd("Health3", new HealthState { Status = HealthStatus.Healthy, Description = "Health Check Three" });

            services.AddSingleton(sp => hcd);

            var checks = services.AddHealthChecks();
            foreach (var check in hcd.Checks)
            {
                checks.AddCheck(check.Key, () => new HealthCheckResult(check.Value.Status, check.Value.Description));
            }
        }
    }
}
