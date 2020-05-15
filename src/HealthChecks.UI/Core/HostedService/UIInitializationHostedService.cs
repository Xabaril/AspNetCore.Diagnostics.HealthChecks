using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.HostedService
{
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
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing UI Database");
            var scopeFactory = _provider.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();

            await InitializeDatabase(scope.ServiceProvider);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task InitializeDatabase(IServiceProvider sp)
        {
            var context = sp.GetRequiredService<HealthChecksDb>();
            var configuration = sp.GetRequiredService<IConfiguration>();
            var settings = sp.GetRequiredService<IOptions<Settings>>();

            if (await ShouldMigrateDatabase(context))
            {
                _logger.LogInformation("Executing database migrations");
                await context.Database.MigrateAsync();
            }

            var healthCheckConfigurations = settings.Value?
                                .HealthChecks?
                                .Select(s => new HealthCheckConfiguration
                                {
                                    Name = s.Name,
                                    Uri = s.Uri
                                });

            bool isInitialized = await context.Configurations.AnyAsync();

            if (!isInitialized && healthCheckConfigurations.Any())
            {
                _logger.LogInformation("Saving healthchecks configuration to database");

                await context.Configurations
                     .AddRangeAsync(healthCheckConfigurations);

            }
            else if (isInitialized && healthCheckConfigurations.Any())
            {
                var dbConfigurations = await context.Configurations.ToListAsync();

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
                        await context.AddAsync(item);
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task<bool> ShouldMigrateDatabase(HealthChecksDb context)
        {
            return (!_settings.DisableMigrations &&
                !context.Database.IsInMemory() &&
                (await context.Database.GetPendingMigrationsAsync()).Any());
        }
    }

}
