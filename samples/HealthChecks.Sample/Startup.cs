using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /*
             * If you have different hosted services, please check this open bug on 2.2 HealthChecks
             * https://github.com/aspnet/Extensions/issues/639 and the workaround proposed by @NatMarchand
             * or register all hosted service before call AddHealthChecks.
             */

            services
                .AddApplicationInsightsTelemetry()
                .AddHealthChecks()
                //.AddRabbitMQ(rabbitConnectionString: "amqp://localhost:5672", name: "rabbit1")
                //.AddRabbitMQ(rabbitConnectionString: "amqp://localhost:6672", name: "rabbit2")
                //.AddSqlServer(connectionString: Configuration["Data:ConnectionStrings:Sample"])
                .AddCheck<RandomHealthCheck>("random")
                //.AddIdentityServer(new Uri("http://localhost:6060"))
                //.AddAzureServiceBusQueue("Endpoint=sb://unaidemo.servicebus.windows.net/;SharedAccessKeyName=policy;SharedAccessKey=5RdimhjY8yfmnjr5L9u5Cf0pCFkbIM7u0HruJuhjlu8=", "que1")
                //.AddAzureServiceBusTopic("Endpoint=sb://unaidemo.servicebus.windows.net/;SharedAccessKeyName=policy;SharedAccessKey=AQhdhXwnkzDO4Os0abQV7f/kB6esTfz2eFERMYKMsKk=", "to1")
                .AddApplicationInsightsPublisher(saveDetailedReport: true);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Sample")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true
                })
                .UseHealthChecks("/healthz", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                })
                .UseHealthChecksPrometheusExporter("/metrics")
                .UseRouting()
                .UseEndpoints(config => config.MapDefaultControllerRoute());
        }

        public class RandomHealthCheck : IHealthCheck
        {
            public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            {
                if (DateTime.UtcNow.Minute % 2 == 0)
                {
                    return Task.FromResult(HealthCheckResult.Healthy());
                }

                return Task.FromResult(HealthCheckResult.Unhealthy(description: "failed", exception: new InvalidCastException("Invalid cast from to to to")));
            }

        }
    }
}
