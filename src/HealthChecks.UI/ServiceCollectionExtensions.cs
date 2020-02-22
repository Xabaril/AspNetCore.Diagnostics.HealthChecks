using HealthChecks.UI;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
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
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthChecksUI(this IServiceCollection services, string databaseName = "healthchecksdb", Action<Settings> setupSettings = null)
        {
            var configuration = services.BuildServiceProvider()
                              .GetService<IConfiguration>();
            var healthCheckSettings = services.BuildServiceProvider()
        .GetService<IOptions<Settings>>()
        .Value ?? new Settings();
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
            services.AddHealthChecksUI<HealthChecksDb>(setupSettings);
         
            return services;
        }

        public static IServiceCollection AddHealthChecksUI<T>(this IServiceCollection services,  Action<Settings> setupSettings = null) where T : DbContext, IHealthChecksDb
        {
            var configuration = services.BuildServiceProvider()
         .GetService<IConfiguration>();
            services
                .AddOptions()
                .Configure<Settings>(settings =>
                {
                    configuration.BindUISettings(settings);
                    setupSettings?.Invoke(settings);
                })
                .Configure<KubernetesDiscoverySettings>(settings =>
                {
                    configuration.Bind(Keys.HEALTHCHECKSUI_KUBERNETES_DISCOVERY_SETTING_KEY, settings);
                })
                .AddSingleton<ServerAddressesService>()
                .AddSingleton<IHostedService, HealthCheckCollectorHostedService>()
                .AddScoped<IHealthCheckFailureNotifier<T>, WebHookFailureNotifier<T>>()
                .AddScoped<IHealthCheckReportCollector, HealthCheckReportCollector<T>>()
                .AddApiEndpointHttpClient()
                .AddWebhooksEndpointHttpClient();

            var kubernetesDiscoverySettings = services.BuildServiceProvider()
                .GetService<IOptions<KubernetesDiscoverySettings>>()
                .Value ?? new KubernetesDiscoverySettings();

            if (kubernetesDiscoverySettings.Enabled)
            {
                services.AddSingleton(kubernetesDiscoverySettings)
                    .AddHostedService<KubernetesDiscoveryHostedService<T>>()
                    .AddHttpClient(Keys.K8S_DISCOVERY_HTTP_CLIENT_NAME, (provider, client) => client.ConfigureKubernetesClient(provider))
                        .ConfigureKubernetesMessageHandler()
                    .Services
                    .AddHttpClient(Keys.K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME);
            }
            var serviceProvider = services.BuildServiceProvider();
            CreateDatabase<T>(serviceProvider).Wait();
            return services;
        }

        public static IServiceCollection AddApiEndpointHttpClient(this IServiceCollection services)
        {
            return services.AddHttpClient(Keys.HEALTH_CHECK_HTTP_CLIENT_NAME, (sp, client) =>
            {
                var settings = sp.GetService<IOptions<Settings>>();
                settings.Value.ApiEndpointHttpClientConfig?.Invoke(sp, client);
            })
              .ConfigurePrimaryHttpMessageHandler(sp =>
              {
                  var settings = sp.GetService<IOptions<Settings>>();
                  return settings.Value.ApiEndpointHttpHandler?.Invoke(sp) ?? new HttpClientHandler();
              })
             .Services;
        }

        public static IServiceCollection AddWebhooksEndpointHttpClient(this IServiceCollection services)
        {
            return services.AddHttpClient(Keys.HEALTH_CHECK_WEBHOOK_HTTP_CLIENT_NAME, (sp, client) =>
            {
                var settings = sp.GetService<IOptions<Settings>>();
                settings.Value.WebHooksEndpointHttpClientConfig?.Invoke(sp, client);
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
             {
                 var settings = sp.GetService<IOptions<Settings>>();
                 return settings.Value.WebHooksEndpointHttpHandler?.Invoke(sp) ?? new HttpClientHandler();
             })
            .Services;
        }

        static async Task CreateDatabase<T>(IServiceProvider serviceProvider) where T : DbContext, IHealthChecksDb
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetService<T>();

                var configuration = scope.ServiceProvider
                    .GetService<IConfiguration>();

                var settings = scope.ServiceProvider
                    .GetService<IOptions<Settings>>();
                if (db.Database.GetMigrations().Count()>0)
                {
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
                        var hcc = healthCheckConfigurations.ToDictionary(t => t.Name);
                        var comparer = new HealthCheckConfigurationEqualityComparer();
                        HealthCheckConfiguration[] second = db.Configurations.AsNoTracking().ToArray();
                        var needadd = healthCheckConfigurations.Except(second, comparer);
                        if (needadd.Any())
                        {
                            await db.Configurations
                           .AddRangeAsync(needadd);
                        }
                        var needremove = second.Except(healthCheckConfigurations, comparer);
                        if (needremove.Any())
                        {
                            db.Configurations.RemoveRange(needremove);
                        }
                        var needupdate = second.Intersect(healthCheckConfigurations, comparer);
                        needupdate.ToList().ForEach(c =>
                        {
                            if (c.Uri != hcc[c.Name].Uri) c.Uri = hcc[c.Name].Uri;
                            if (c.DiscoveryService != hcc[c.Name].DiscoveryService) c.DiscoveryService = hcc[c.Name].DiscoveryService;
                        });

                        await db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}