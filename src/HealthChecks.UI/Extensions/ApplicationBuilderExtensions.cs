using HealthChecks.UI;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using HealthChecks.UI.Middleware;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHealthChecksUI(this IApplicationBuilder app, Action<Options> setup)
    {
        var options = new Options();
        setup?.Invoke(options);

        return ConfigurePipeline(app, options);
    }

    public static IApplicationBuilder UseHealthChecksUI(this IApplicationBuilder app)
    {
        return ConfigurePipeline(app, new Options());
    }

    private static IApplicationBuilder ConfigurePipeline(IApplicationBuilder app, Options options)
    {
        EnsureValidApiOptions(options,
            (app.ApplicationServices.GetService(typeof(IEnumerable<EndpointDataSource>)) as IEnumerable<EndpointDataSource>)
                ?.SelectMany(dataSource => dataSource.Endpoints)
                ?.OfType<RouteEndpoint>());

        var embeddedResourcesAssembly = typeof(UIResource).Assembly;

        app.Map(options.ApiPath, appBuilder =>
        {
            appBuilder
            .UseMiddleware<UIApiRequestLimitingMiddleware>()
            .UseMiddleware<UIApiEndpointMiddleware>();
        });

        app.Map(options.WebhookPath, appBuilder => appBuilder.UseMiddleware<UIWebHooksApiMiddleware>());

        app.Map($"{options.ApiPath}/{Keys.HEALTHCHECKSUI_SETTINGS_PATH}", appBuilder => appBuilder.UseMiddleware<UISettingsMiddleware>());

        new UIResourcesMapper(
            new UIEmbeddedResourcesReader(embeddedResourcesAssembly))
            .Map(app, options);

        return app;
    }

    private static void EnsureValidApiOptions(Options options, IEnumerable<RouteEndpoint>? routeEndpoints)
    {
        Action<string, string> ensureValidPath = (string path, string argument) =>
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/"))
            {
                throw new ArgumentException("The value for customized path can't be null and need to start with / character.", argument);
            }
        };

        Func<string, string> normalizeUriPath = (string path) =>
            path.TrimEnd('/').ToLower();

        ensureValidPath(options.ApiPath, nameof(Options.ApiPath));
        ensureValidPath(options.UIPath, nameof(Options.UIPath));
        ensureValidPath(options.WebhookPath, nameof(Options.WebhookPath));

        if (routeEndpoints
            ?.Select(endPoint => normalizeUriPath(endPoint.RoutePattern.RawText ?? string.Empty))
            ?.Count(path => path == normalizeUriPath(options.ApiPath)) > 0)
        {
            throw new ArgumentException("ApiPath should not match any route registered via MapHealthChecks!");
        }
    }
}
