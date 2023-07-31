using System.Text.Json;
using System.Text.Json.Serialization;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Middleware;

internal class UIApiEndpointMiddleware
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Settings _settings;

    public UIApiEndpointMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, IOptions<Settings> settings)
    {
        _ = next;
        _serviceScopeFactory = serviceScopeFactory;
        _settings = Guard.ThrowIfNull(settings?.Value);
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                // allowIntegerValues: true https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/1422
                new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: true)
            }
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<HealthChecksDb>();

        var healthChecks = await db.Configurations.ToListAsync().ConfigureAwait(false);

        var healthChecksExecutions = new List<HealthCheckExecution>();

        foreach (var item in healthChecks.OrderBy(h => h.Id))
        {
            var execution = await db.Executions
                        .Include(le => le.Entries)
                        .Where(le => le.Name == item.Name)
                        .AsNoTracking()
                        .SingleOrDefaultAsync()
                        .ConfigureAwait(false);

            if (execution != null)
            {
                execution.History = await db.HealthCheckExecutionHistories
                    .Where(eh => EF.Property<int>(eh, "HealthCheckExecutionId") == execution.Id)
                    .OrderByDescending(eh => eh.On)
                    .Take(_settings.MaximumExecutionHistoriesPerEndpoint)
                    .ToListAsync()
                    .ConfigureAwait(false);

                healthChecksExecutions.Add(execution);
            }
        }

        if (_settings.ConfigureUIApiEndpointResult != null)
            await _settings.ConfigureUIApiEndpointResult(healthChecksExecutions).ConfigureAwait(false);
        await context.Response.WriteAsJsonAsync(healthChecksExecutions, _jsonSerializerOptions).ConfigureAwait(false);
    }
}
