using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UIAndApi
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
            //
            //  This project configure health checks for asp.net core project and UI
            //  in the same project. Typically health checks and UI are on different projects
            //  UI exist also as container image
            //

            services
                .AddHealthChecksUI()
                .AddInMemoryStorage()
                .Services
                .AddHealthChecks()
                .AddCheck<RandomHealthCheck>("random")
                .AddUrlGroup(new Uri("http://httpbin.org/status/200"))
                //.AddKubernetes(setup =>
                //{
                //    setup.WithConfiguration(k8s.KubernetesClientConfiguration.BuildConfigFromConfigFile())
                //        .CheckDeployment("wordpress-one-wordpress",
                //            d => d.Status.Replicas == 2 && d.Status.ReadyReplicas == 2)
                //        .CheckService("wordpress-one-wordpress", s => s.Spec.Type == "LoadBalancer")
                //        .CheckPod("myapp-pod", p =>  p.Metadata.Labels["app"] == "myapp" );
                //})
                .Services
                .AddControllers();

            //
            //   below show howto use default policy handlers ( polly )
            //   with httpclient on asp.net core also
            //   on uri health checks
            //

            //var retryPolicy = HttpPolicyExtensions
            //    .HandleTransientHttpError()
            //    .Or<TimeoutRejectedException>()
            //    .RetryAsync(5);

            //services.AddHttpClient("uri-group") //default healthcheck registration name for uri ( you can change it on AddUrlGroup )
            //    .AddPolicyHandler(retryPolicy)
            //    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            //    {
            //        ClientCertificateOptions = ClientCertificateOption.Manual,
            //        ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
            //        {
            //            return true;
            //        }
            //    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseRouting()
                .UseEndpoints(config =>
                {
                    config.MapHealthChecks("healthz", new HealthCheckOptions()
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    config.MapHealthChecksUI();
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

            return Task.FromResult(HealthCheckResult.Unhealthy(description: "failed"));
        }
    }
}
