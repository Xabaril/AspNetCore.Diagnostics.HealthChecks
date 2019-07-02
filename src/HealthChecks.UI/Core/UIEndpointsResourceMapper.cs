using System;
using System.Threading.Tasks;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HealthChecks.UI.Core
{
    internal class UIEndpointsResourceMapper
    {
        private readonly IUIResourcesReader _reader;

        public UIEndpointsResourceMapper(IUIResourcesReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public void Map(IEndpointRouteBuilder builder, Options options)
        {
            var resources = _reader.UIResources;
            var ui = resources.GetMainUI(options);

            foreach (var resource in resources)
            {
                builder.MapGet($"{options.ResourcesPath}/{resource.FileName}", async context =>
                {
                    context.Response.ContentType = resource.ContentType;
                    await context.Response.WriteAsync(resource.Content);
                });
            }

            builder.MapGet($"{options.UIPath}", async context =>
            {
                context.Response.OnStarting(() =>
                {

                    if (!context.Response.Headers.ContainsKey("Cache-Control"))
                    {
                        context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    }

                    return Task.CompletedTask;
                });

                context.Response.ContentType = ui.ContentType;
                await context.Response.WriteAsync(ui.Content);

            });
        }
    }
}