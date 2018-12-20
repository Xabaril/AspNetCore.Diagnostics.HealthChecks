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

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthChecksUI(this IServiceCollection services, string databaseName = "healthchecksdb")
        {
            var configuration = services.BuildServiceProvider()
                .GetService<IConfiguration>();

            services.AddOptions();
            services.Configure<Settings>((settings) =>
            {
                configuration.Bind(Keys.HEALTHCHECKSUI_SECTION_SETTING_KEY, settings);
            });

            services.AddHttpClient(Keys.HEALTH_CHECK_HTTP_CLIENT_NAME)
                    .Services
                    .AddSingleton<IHostedService, HealthCheckCollectorHostedService>()
                    .AddScoped<IHealthCheckFailureNotifier, WebHookFailureNotifier>()
                    .AddScoped<IHealthCheckReportCollector, HealthCheckReportCollector>()
                    .AddDbContext<HealthChecksDb>(db =>
                    {
                        var contentRoot = configuration[HostDefaults.ContentRootKey];
                        var path = Path.Combine(Path.GetDirectoryName(contentRoot), databaseName);
                        db.UseSqlite($"Data Source={path}");
                    });

            var kubernetesDiscoveryOptions = new KubernetesDiscoveryOptions();
            configuration.Bind(Keys.HEALTHCHECKSUI_KUBERNETES_DISCOVERY_SETTING_KEY, kubernetesDiscoveryOptions);

            if (kubernetesDiscoveryOptions.Enabled)
            {
                services.AddSingleton(kubernetesDiscoveryOptions)
                        
                        .AddHttpClient(Keys.K8S_DISCOVERY_HTTP_CLIENT_NAME,
                                (provider, client) => client.ConfigureKubernetesClient(provider))
                                .ConfigureKubernetesMessageHandler()
                        .Services
                        .AddHttpClient(Keys.K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME)
                        .Services
                        .AddHostedService<KubernetesDiscoveryHostedService>();
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

                var liveness = settings.Value?
                    .HealthChecks?
                    .Select(s => new HealthCheckConfiguration()
                    {
                        Name = s.Name,
                        Uri = s.Uri
                    });

                if (liveness != null
                    &&
                    liveness.Any())
                {

                    await db.Configurations
                        .AddRangeAsync(liveness);

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}