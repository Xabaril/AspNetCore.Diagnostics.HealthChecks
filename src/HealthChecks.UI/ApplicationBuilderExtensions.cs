using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Middleware;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHealthChecksUI<T>(this IApplicationBuilder app, Action<Options> setup) where T : DbContext, IHealthChecksDb
        {
            var options = new Options();
            setup?.Invoke(options);
            return ConfigurePipeline<T>(app, options);
        }
        public static IApplicationBuilder UseHealthChecksUI(this IApplicationBuilder app, Action<Options> setup)
        {
            var options = new Options();
            setup?.Invoke(options);

            return ConfigurePipeline<HealthChecksDb>(app, options);
        }
        public static IApplicationBuilder UseHealthChecksUI<T>(this IApplicationBuilder app) where T : DbContext, IHealthChecksDb
        {
            return ConfigurePipeline<T>(app, new Options());
        }
        public static IApplicationBuilder UseHealthChecksUI(this IApplicationBuilder app)
        {
            return ConfigurePipeline<HealthChecksDb>(app, new Options());
        }
        
            private static IApplicationBuilder ConfigurePipeline<T>(IApplicationBuilder app, Options options) where T : DbContext, IHealthChecksDb
        {
            EnsureValidApiOptions(options);

            var embeddedResourcesAssembly = typeof(UIResource).Assembly;

            app.Map(options.ApiPath, appBuilder => appBuilder.UseMiddleware<UIApiEndpointMiddleware<T>>());
            app.Map(options.WebhookPath, appBuilder => appBuilder.UseMiddleware<UIWebHooksApiMiddleware>());

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
                    throw new ArgumentException("The value for customized path can't be null and need to start with / character.", argument);
                }
            };

            ensureValidPath(options.ApiPath, nameof(Options.ApiPath));
            ensureValidPath(options.UIPath, nameof(Options.UIPath));
            ensureValidPath(options.WebhookPath, nameof(Options.WebhookPath));
        }
    }
}
