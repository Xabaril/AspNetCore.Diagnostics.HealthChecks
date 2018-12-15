using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;

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
                .AddHealthChecks()
                .AddUrlGroup(new Uri("http://httpbin.org/status/500"))
                .Services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
            //    .AddPolicyHandler(retryPolicy);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
           app.UseHealthChecks("/healthz",new HealthCheckOptions()
           {
               Predicate = _=>true,
               ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
           })
           .UseHealthChecksUI()
           .UseMvc();
        }
    }
}
