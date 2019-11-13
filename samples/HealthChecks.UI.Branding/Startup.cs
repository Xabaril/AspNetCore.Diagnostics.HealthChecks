using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.UI.Branding
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHealthChecks()
                .AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 100)
                .AddCheck<RandomHealthCheck>("random1")
                .AddCheck<RandomHealthCheck>("random2")
                .Services
                .AddHealthChecksUI(setupSettings: setup =>
                {
                    setup.AddHealthCheckEndpoint("endpoint1", "http://localhost:8001/healthz");
                    setup.AddWebhookNotification("webhook1", uri: "http://httpbin.org/status/200?code=ax3rt56s", payload: "{ message: \"Webhook report for [[LIVENESS]]: [[FAILURE]]\"}");
                })
                .AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting()
               .UseEndpoints(config =>
               {
                   config.MapHealthChecks("/healthz", new HealthCheckOptions
                   {
                       Predicate = _ => true,
                       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                   });

                   config.MapHealthChecksUI(setup =>
                   {
                       setup.AddCustomStylesheet("dotnet.css");
                   });

                   config.MapDefaultControllerRoute();
               });
        }
    }

    public class RandomHealthCheck
    : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (DateTime.UtcNow.Minute % 2 == 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(description: $"The healthcheck {context.Registration.Name} failed at minute {DateTime.UtcNow.Minute}"));
        }
    }
}