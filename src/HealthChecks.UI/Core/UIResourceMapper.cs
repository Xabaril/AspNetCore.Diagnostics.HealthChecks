using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core
{
    internal class UIResourcesMapper
    {
        private readonly IUIResourcesReader _reader;

        public UIResourcesMapper(IUIResourcesReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public void Map(IApplicationBuilder app, Options options)
        {
            var resources = _reader.UIResources;
            var UIMain = resources.GetMainUI(options);

            foreach (var resource in resources)
            {
                app.Map($"{Keys.HEALTHCHECKSUI_RESOURCES_PATH}/{resource.FileName}", appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.ContentType = resource.ContentType;
                        await context.Response.WriteAsync(resource.Content);
                    });
                });
            }

            app.Map($"{options.UIPath}", appBuilder =>
            {
                appBuilder.Run(context =>
                {
                    context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    context.Response.ContentType = UIMain.ContentType;
                    context.Response.WriteAsync(UIMain.Content);
                    return Task.CompletedTask;
                });
            });
        }
    }
}
