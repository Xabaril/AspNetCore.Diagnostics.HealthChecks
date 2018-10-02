using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using HealthChecks.UI.Middleware;
using Microsoft.AspNetCore.Builder;
using System;

namespace HealthChecks.UI
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHealthChecksUI(this IApplicationBuilder app, Action<Options> setup)
        {
            var options = new Options();
            setup?.Invoke(options);

            return ConfigurePipeline(app, options);
        }

        private static IApplicationBuilder ConfigurePipeline(IApplicationBuilder app, Options options)
        {
            EnsureValidApiOptions(options);

            var embeddedResourcesAssembly = typeof(UIResource).Assembly;

            app.Map(options.ApiPath, appBuilder => appBuilder.UseMiddleware<UIApiEndpointMiddleware>());
            app.Map(options.UIPath, appBuilder => appBuilder.UseMiddleware<UIWebHooksApiMiddleware>());

            new UIResourcesMapper(
                new UIEmbeddedResourcesReader(embeddedResourcesAssembly))
                .Map(app, options);

            return app;
        }

        private static void EnsureValidApiOptions(Options options)
        {
            Action<string, string> ensureValidPath = (string path, string argument) =>
             {
                 if (string.IsNullOrEmpty(path) || !path.StartsWith("/"))
                 {
                     throw new ArgumentNullException(argument);
                 }
             };

            ensureValidPath(options.ApiPath, nameof(Options.ApiPath));
            ensureValidPath(options.UIPath, nameof(Options.UIPath));
            ensureValidPath(options.WebhookPath, nameof(Options.WebhookPath));
        }
    }
}
