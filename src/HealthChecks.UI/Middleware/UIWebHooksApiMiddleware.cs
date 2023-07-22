using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core;

public class UIWebHooksApiMiddleware
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UIWebHooksApiMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
    {
        _ = next;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>();
        var sanitizedWebhooksResponse = settings.Value.Webhooks.Select(item => new
        {
            item.Name,
            Payload = string.IsNullOrEmpty(item.Payload) ? new JsonObject() : JsonNode.Parse(Regex.Unescape(item.Payload))
        });

        await context.Response.WriteAsJsonAsync(sanitizedWebhooksResponse, _jsonSerializerOptions).ConfigureAwait(false);
    }
}
