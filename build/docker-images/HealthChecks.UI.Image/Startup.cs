using HealthChecks.UI.Image.Configuration;
using HealthChecks.UI.Image.PushService;
using Microsoft.AspNetCore.Mvc;

namespace HealthChecks.UI.Image
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
#pragma warning disable ASP5001, CS0618 // Type or member is obsolete
            services
                .AddHealthChecksUI()
                .AddStorageProvider(Configuration)
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
#pragma warning restore ASP5001, CS0618 // Type or member is obsolete

            if (bool.TryParse(Configuration[PushServiceKeys.Enabled], out bool enabled) && enabled)
            {
                if (string.IsNullOrEmpty(Configuration[PushServiceKeys.PushEndpointSecret]))
                {
                    throw new Exception($"{PushServiceKeys.PushEndpointSecret} environment variable has not been configured");
                }
                services.AddTransient<HealthChecksPushService>();
            }
        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting()
                .UseEndpoints(config =>
                {
                    config.MapHealthChecksUI(Configuration);
                    config.MapDefaultControllerRoute();
                });
        }
    }
}
