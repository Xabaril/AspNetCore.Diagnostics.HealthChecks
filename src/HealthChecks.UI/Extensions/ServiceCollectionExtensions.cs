using HealthChecks.UI;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Discovery.K8S;
using HealthChecks.UI.Core.HostedService;
using HealthChecks.UI.Core.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static HealthChecksUIBuilder AddHealthChecksUI(this IServiceCollection services, Action<Settings> setupSettings = null)
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

            services.TryAddSingleton<ServerAddressesService>();
            services.TryAddScoped<IHealthCheckFailureNotifier, WebHookFailureNotifier>();
            services.TryAddScoped<IHealthCheckReportCollector, HealthCheckReportCollector>();

            services
                .AddHostedService<UIInitializationHostedService>()
                .AddHostedService<HealthCheckCollectorHostedService>()
                .AddKubernetesDiscoveryService()
                .AddApiEndpointHttpClient()
                .AddWebhooksEndpointHttpClient();

            return new HealthChecksUIBuilder(services);
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

        static IServiceCollection AddKubernetesDiscoveryService(this IServiceCollection services)
        {
            services
                .AddHostedService<KubernetesDiscoveryHostedService>()
                .AddHttpClient(Keys.K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME);

            return services;
        }
    }
}