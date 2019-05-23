using HealthChecks.UI;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Discovery.K8S;
using HealthChecks.UI.Core.Discovery.K8S.Extensions;
using HealthChecks.UI.Core.HostedService;
using HealthChecks.UI.Core.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Core.Discovery;
using HealthChecks.UI.Core.Discovery.Docker;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthChecksUI(this IServiceCollection services, string databaseName = "healthchecksdb", Action<Settings> setupSettings = null)
        {
            var configuration = services.BuildServiceProvider()
                .GetService<IConfiguration>();

            services
                .AddOptions()
                .Configure<Settings>(settings =>
                {
                    configuration.GetSection(Keys.HEALTHCHECKSUI_SECTION_SETTING_KEY)
                        .Bind(settings, c => c.BindNonPublicProperties = true);

                    setupSettings?.Invoke(settings);
                })
                .Configure<KubernetesDiscoverySettings>(settings =>
                {
                    configuration.Bind(Keys.HEALTHCHECKSUI_KUBERNETES_DISCOVERY_SETTING_KEY, settings);
                })
                .Configure<DockerDiscoverySettings>(settings =>
                {
                    configuration.Bind(Keys.HEALTHCHECKSUI_DOCKER_DISCOVERY_SETTING_KEY, settings);
                })
                .AddSingleton<IHostedService, HealthCheckCollectorHostedService>()
                .AddScoped<IHealthCheckFailureNotifier, WebHookFailureNotifier>()
                .AddScoped<IHealthCheckReportCollector, HealthCheckReportCollector>()
                .AddHttpClient(Keys.HEALTH_CHECK_HTTP_CLIENT_NAME);

            var healthCheckSettings = services.BuildServiceProvider()
                .GetService<IOptions<Settings>>()
                .Value ?? new Settings();

            var kubernetesDiscoverySettings = services.BuildServiceProvider()
                .GetService<IOptions<KubernetesDiscoverySettings>>()
                .Value ?? new KubernetesDiscoverySettings();

            var dockerDiscoverySettings = services.BuildServiceProvider()
                .GetService<IOptions<DockerDiscoverySettings>>()
                .Value ?? new DockerDiscoverySettings();

            services.AddDbContext<HealthChecksDb>(db =>
            {
                var connectionString = healthCheckSettings.HealthCheckDatabaseConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    var contentRoot = configuration[HostDefaults.ContentRootKey];
                    var path = Path.Combine(contentRoot, databaseName);
                    connectionString = $"Data Source={path}";
                }
                else
                {
                    connectionString = Environment.ExpandEnvironmentVariables(connectionString);
                }
                db.UseSqlite(connectionString);
            });

            if (kubernetesDiscoverySettings.Enabled)
            {
                services.AddSingleton(kubernetesDiscoverySettings)
                    .AddHostedService<KubernetesDiscoveryHostedService>()
                    .AddHttpClient(Keys.K8S_DISCOVERY_HTTP_CLIENT_NAME, (provider, client) => client.ConfigureKubernetesClient(provider))
                    .ConfigureKubernetesMessageHandler()
                    .Services
                    .AddHttpClient(Keys.K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME);
            }

            if (dockerDiscoverySettings.Enabled)
            {
                services.AddHostedService<DockerDiscoveryHostedService>()
                    .AddScoped<IDiscoveryRegistryService, DiscoveryRegistryService>()
                    .AddSingleton<IDockerDiscoveryService>(x =>
                    {
                        var config = x.GetRequiredService<IOptions<DockerDiscoverySettings>>().Value;

                        var uri = new Uri(config.Endpoint);
                        var labelPrefix = $"{config.ServicesLabelPrefix}.";

                        return ActivatorUtilities.CreateInstance<DockerDiscoveryService>(x, uri, labelPrefix);
                    })
                    .AddHttpClient(Keys.DISCOVERY_SERVICE_HTTP_CLIENT_NAME);
            }

            var serviceProvider = services.BuildServiceProvider();

            CreateDatabase(serviceProvider).Wait();

            return services;
        }

        static async Task CreateDatabase(IServiceProvider serviceProvider)
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetService<HealthChecksDb>();

                var configuration = scope.ServiceProvider
                    .GetService<IConfiguration>();

                var settings = scope.ServiceProvider
                    .GetService<IOptions<Settings>>();

                await db.Database.EnsureDeletedAsync();
                await db.Database.MigrateAsync();

                var healthCheckConfigurations = settings.Value?
                    .HealthChecks?
                    .Select(s => new HealthCheckConfiguration
                    {
                        Name = s.Name,
                        Uri = s.Uri
                    });

                if (healthCheckConfigurations != null
                    &&
                    healthCheckConfigurations.Any())
                {
                    await db.Configurations
                        .AddRangeAsync(healthCheckConfigurations);

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}