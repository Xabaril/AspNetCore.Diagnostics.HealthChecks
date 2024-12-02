using HealthChecks.UI.Client;
using HealthChecks.UIAndApi.Options;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HealthChecks.UIAndApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        //
        //  This project configure health checks for asp.net core project and UI
        //  in the same project with some ui path customizations.
        //

        services.Configure<RemoteOptions>(options => _configuration.Bind(options));

        services
            .AddHealthChecksUI()
            .AddInMemoryStorage()
//            .AddHealthChecksUI(setupSettings: settings =>
//            {
//                settings
//                    .AddHealthCheckEndpoint("api1", "http://localhost:8001/custom/healthz")
//                    .AddWebhookNotification("webhook1", "http://webhook", "mypayload")
//                    .SetEvaluationTimeInSeconds(16);
//            })
        .Services
            .AddHealthChecks()
            .AddUrlGroup(new Uri("http://httpbin.org/status/200"), name: "uri-1")
            .AddUrlGroup(new Uri("http://httpbin.org/status/200"), name: "uri-2")
            .AddUrlGroup(
                sp =>
                {
                    var remoteOptions = sp.GetRequiredService<IOptions<RemoteOptions>>().Value;
                    return remoteOptions.RemoteDependency;
                },
                "uri-3")
            .AddUrlGroup(new Uri("http://httpbin.org/status/500"), name: "uri-4")
            .Services
            .AddControllers();
    }

    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Sample")]
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
                    setup.PageTitle = "My wonderful Health Checks UI"; // the page title in <head>
                });

                config.MapDefaultControllerRoute();
            });
    }
}
