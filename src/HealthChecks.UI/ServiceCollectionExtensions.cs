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
            services
                .AddOptions<Settings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.BindUISettings(settings);
                    setupSettings?.Invoke(settings);
                });

            services.AddOptions<KubernetesDiscoverySettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.Bind(Keys.HEALTHCHECKSUI_KUBERNETES_DISCOVERY_SETTING_KEY, settings);
                });

            services
                .AddSingleton<ServerAddressesService>()
                .AddSingleton<IHostedService, HealthCheckCollectorHostedService>()
                .AddScoped<IHealthCheckFailureNotifier, WebHookFailureNotifier>()
                .AddScoped<IHealthCheckReportCollector, HealthCheckReportCollector>()
                .AddApiEndpointHttpClient()
                .AddWebhooksEndpointHttpClient();

            services.AddDbContext<HealthChecksDb>((provider, db) =>
            {
                var healthCheckSettings = provider
                      .GetService<IOptions<Settings>>()
                      .Value ?? new Settings();

                var connectionString = healthCheckSettings.HealthCheckDatabaseConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();
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

            services.AddSingleton(provider =>
            {
                var kubernetesDiscoverySettings = provider
                    .GetService<IOptions<KubernetesDiscoverySettings>>()
                    .Value ?? new KubernetesDiscoverySettings();

                return kubernetesDiscoverySettings;
            });

            services
                .AddHostedService<KubernetesDiscoveryHostedService>()
                .AddHttpClient(Keys.K8S_DISCOVERY_HTTP_CLIENT_NAME, (provider, client) => client
                    .ConfigureKubernetesClient(provider))
                    .ConfigureKubernetesMessageHandler()
                .Services
                .AddHttpClient(Keys.K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME);

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
    }
}