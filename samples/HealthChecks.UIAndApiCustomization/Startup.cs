using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HealthChecks.UIAndApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //
            //  This project configure health checks for asp.net core project and UI
            //  in the same project with some ui path customizations. 
            // 

            services
                .AddHealthChecksUI()
                .AddInMemoryStorage()
                //                .AddHealthChecksUI(setupSettings: settings =>
                //                {
                //                    settings
                //                        .AddHealthCheckEndpoint("api1", "http://localhost:8001/custom/healthz")
                //                        .AddWebhookNotification("webhook1", "http://webhook", "mypayload")
                //                        .SetEvaluationTimeInSeconds(16);
                //                })
                .Services
                .AddHealthChecks()
                .AddUrlGroup(new Uri("http://httpbin.org/status/200"), name: "uri-1")
                .AddUrlGroup(new Uri("http://httpbin.org/status/200"), name: "uri-2")
                .AddUrlGroup(new Uri("http://httpbin.org/status/500"), name: "uri-3")
                .Services
                .AddControllers();
        }

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
                        setup.UIPath = "/show-health-ui"; // this is ui path in your browser
                        setup.ApiPath = "/health-ui-api"; // the UI ( spa app )  use this path to get information from the store ( this is NOT the healthz path, is internal ui api )
                    });

                    config.MapDefaultControllerRoute();
                });
        }
    }
}