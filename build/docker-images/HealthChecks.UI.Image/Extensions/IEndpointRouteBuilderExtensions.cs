using HealthChecks.UI.Image;
using HealthChecks.UI.Image.Configuration;
using HealthChecks.UI.Image.Extensions;
using HealthChecks.UI.Image.PushService;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;

namespace Microsoft.AspNetCore.Builder
{
    public static class IEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapHealthChecksUI(this IEndpointRouteBuilder builder,
            IConfiguration configuration)
        {
            if (bool.TryParse(configuration[PushServiceKeys.Enabled], out bool enabled) && enabled)
            {
                builder.MapHealthCheckPushEndpoint(configuration);
            }

            return builder.MapHealthChecksUI(setup =>
            {
                setup.ConfigureStylesheet(configuration);
                setup.ConfigurePaths(configuration);

            });
        }
        private static void MapHealthCheckPushEndpoint(this IEndpointRouteBuilder builder,
            IConfiguration configuration)
        {

            var logger = builder.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("HealthChecks Push Endpoint Enabled");

            builder.MapPost("/healthchecks/push", async context =>
            {
                if (context.Request.IsAuthenticated())
                {
                    using var streamReader = new StreamReader(context.Request.Body);
                    var content = await streamReader.ReadToEndAsync();

                    var endpoint = JsonDocument.Parse(content);
                    var root = endpoint.RootElement;
                    var name = root.GetProperty("name").GetString();
                    var uri = root.GetProperty("uri").GetString();
                    var type = root.GetProperty("type").GetInt16();

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(uri))
                    {
                        var pushService = context.RequestServices.GetService<HealthChecksPushService>();

                        if (type == PushServiceKeys.ServiceAdded)
                        {
                            await pushService.AddAsync(name, uri);
                        }
                        else if (type == PushServiceKeys.ServiceRemoved)
                        {
                            await pushService.RemoveAsync(name);
                        }
                        else if (type == PushServiceKeys.ServiceUpdated)
                        {
                            await pushService.UpdateAsync(name, uri);
                        }
                    }
                }
                else
                {
                    context.Response.StatusCode = 401;
                }

            });
        }
    }
}