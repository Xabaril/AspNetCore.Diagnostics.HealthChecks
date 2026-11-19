using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.UI.Tests;

public class HostBuilderHelper
{
    public static async Task<WebApplication> CreateAsync(ManualResetEventSlim hostReset, ManualResetEventSlim? collectorReset = null, Action<HealthChecksUIBuilder>? configureUI = null)
    {
        var appBuilder = WebApplication.CreateBuilder();
        var uiBuilder = appBuilder.Services
            .AddRouting()
            .AddHealthChecks()
            .AddCheck("check1", () => HealthCheckResult.Healthy())
            .Services
            .AddHealthChecksUI(setup => setup.AddHealthCheckEndpoint(ProviderTestHelper.Endpoints[0].Name, ProviderTestHelper.Endpoints[0].Uri));

        configureUI?.Invoke(uiBuilder);

        if (collectorReset != null)
        {
            appBuilder.Services.AddTransient<IHealthCheckCollectorInterceptor>(sp => new TestCollectorInterceptor(collectorReset));
        }

        appBuilder.WebHost.UseTestServer();
        var app = appBuilder.Build();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = r => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecksUI();

        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(() => hostReset.Set());

        await app.StartAsync();
        return app;
    }
}
