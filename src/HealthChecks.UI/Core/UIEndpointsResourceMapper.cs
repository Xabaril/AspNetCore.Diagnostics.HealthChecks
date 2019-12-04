using System;
using System.Collections.Generic;
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

        public IEnumerable<IEndpointConventionBuilder> Map(IEndpointRouteBuilder builder, Options options)
        {
            var endpoints = new List<IEndpointConventionBuilder>();

            var resources = _reader.UIResources;
            var ui = resources.GetMainUI(options);
            var styleSheets = ui.GetCustomStylesheets(options);

            foreach (var resource in resources)
            {
                endpoints.Add(builder.MapGet($"{options.ResourcesPath}/{resource.FileName}", async context =>
                {
                    context.Response.ContentType = resource.ContentType;
                    await context.Response.WriteAsync(resource.Content);
                }));
            }

            endpoints.Add(builder.MapGet($"{options.UIPath}", async context =>
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
            }));

            foreach (var item in styleSheets)
            {
                endpoints.Add(builder.MapGet(item.ResourcePath, async context =>
                {
                    context.Response.ContentType = "text/css";
                    await context.Response.Body.WriteAsync(item.Content, 0, item.Content.Length);
                }));
            }

            return endpoints;
        }
    }
}