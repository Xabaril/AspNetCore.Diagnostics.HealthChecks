using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core;

public class UIWebHooksApiMiddleware
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<UIWebHooksApiMiddleware> _logger;

    public UIWebHooksApiMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, ILogger<UIWebHooksApiMiddleware> logger)
    {
        _ = next;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>();
        var sanitizedWebhooksResponse = settings.Value.Webhooks.Select(item =>
        {
            try
            {
                var payloadObject = string.IsNullOrEmpty(item.Payload) ? new JsonObject() : JsonNode.Parse(ReplaceEscapeSequences(item.Payload));
                return new
                {
                    item.Name,
                    Payload = payloadObject
                };
            }
            catch (JsonException exception)
            {
                var errorMessage = $"UIWebHooksApiMiddleware threw an exception when trying to parse payload for webhook {item.Name}.";
                _logger.LogError(exception, errorMessage);
                throw new Exception(errorMessage, exception);
            }
        });

        await context.Response.WriteAsJsonAsync(sanitizedWebhooksResponse, _jsonSerializerOptions).ConfigureAwait(false);
    }

    public static string ReplaceEscapeSequences(string input)
    {
        // Regular expression to match newline and carriage return escape sequences outside of double quotes
        var pattern = @"(?<!\"")\\([nr])((?=(?:[^""]*\""[^""]*"")*[^""]*$))";

        return Regex.Replace(input, pattern, match =>
        {
            if (match.Groups[1].Value == "n")
            {
                return "\n";
            }
            else if (match.Groups[1].Value == "r")
            {
                return "\r";
            }
            return match.Value; // No change if the match doesn't represent a newline or carriage return
        });
    }

}
