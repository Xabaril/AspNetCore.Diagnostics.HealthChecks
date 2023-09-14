using HealthChecks.UI.Configuration;
using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core.HostedService;

internal class UIInitializationHostedService : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<UIInitializationHostedService> _logger;
    private readonly Settings _settings;

    public UIInitializationHostedService(
        IServiceProvider provider,
        ILogger<UIInitializationHostedService> logger,
        IOptions<Settings> settings)
    {
        _provider = Guard.ThrowIfNull(provider);
        _logger = Guard.ThrowIfNull(logger);
        _settings = Guard.ThrowIfNull(settings?.Value);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing UI Database");
        var scopeFactory = _provider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        await InitializeDatabaseAsync(scope.ServiceProvider).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task InitializeDatabaseAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<HealthChecksDb>();
        var configuration = sp.GetRequiredService<IConfiguration>();
        var settings = sp.GetRequiredService<IOptions<Settings>>();

        if (await ShouldMigrateDatabaseAsync(context).ConfigureAwait(false))
        {
            _logger.LogInformation("Executing database migrations");
            await context.Database.MigrateAsync().ConfigureAwait(false);
        }

        var healthCheckConfigurations = settings.Value?
                            .HealthChecks?
                            .Select(s => new HealthCheckConfiguration
                            {
                                Name = s.Name,
                                Uri = s.Uri
                            });

        bool isInitialized = await context.Configurations.AnyAsync().ConfigureAwait(false);

        if (!isInitialized && healthCheckConfigurations != null && healthCheckConfigurations.Any())
        {
            _logger.LogInformation("Saving healthchecks configuration to database");

            await context.Configurations
                 .AddRangeAsync(healthCheckConfigurations)
                 .ConfigureAwait(false);

        }
        else if (isInitialized && healthCheckConfigurations != null && healthCheckConfigurations.Any())
        {
            var dbConfigurations = await context.Configurations.ToListAsync().ConfigureAwait(false);

            var existingConfigurations = dbConfigurations
                .Where(hc => healthCheckConfigurations.Any(dbc => string.Equals(hc.Name, dbc.Name, StringComparison.InvariantCultureIgnoreCase)));

            foreach (var item in existingConfigurations)
            {
                var uri = healthCheckConfigurations.First(hc => hc.Name == item.Name).Uri;
                if (!uri.Equals(item.Uri, StringComparison.InvariantCultureIgnoreCase))
                {
                    item.Uri = uri;
                    _logger.LogInformation("Updating service {service} to new uri: {uri}", item.Name, item.Uri);
                }
            }

            var newConfigurations = healthCheckConfigurations
                .Where(hc => !dbConfigurations.Any(dbc => string.Equals(hc.Name, dbc.Name, StringComparison.InvariantCultureIgnoreCase)));

            if (newConfigurations != null)
            {
                foreach (var item in newConfigurations)
                {
                    _logger.LogInformation("Adding new service {service} to database", item.Name);
                    await context.AddAsync(item).ConfigureAwait(false);
                }
            }
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    private async Task<bool> ShouldMigrateDatabaseAsync(HealthChecksDb context)
    {
        return !_settings.DisableMigrations &&
            !context.Database.IsInMemory() &&
            (await context.Database.GetPendingMigrationsAsync().ConfigureAwait(false)).Any();
    }
}
