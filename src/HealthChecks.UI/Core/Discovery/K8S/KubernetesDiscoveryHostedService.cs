using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Discovery.K8S.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.Discovery.K8S
{
    internal class KubernetesDiscoveryHostedService : IHostedService
    {
        private readonly KubernetesDiscoverySettings _discoveryOptions;
        private readonly ILogger<KubernetesDiscoveryHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _discoveryClient;
        private readonly HttpClient _clusterServiceClient;
        private readonly KubernetesAddressFactory _addressFactory;

        private Task _executingTask;

        public KubernetesDiscoveryHostedService(
            IServiceProvider serviceProvider,
            KubernetesDiscoverySettings discoveryOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<KubernetesDiscoveryHostedService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _discoveryOptions = discoveryOptions ?? throw new ArgumentNullException(nameof(discoveryOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _discoveryClient = httpClientFactory.CreateClient(Keys.K8S_DISCOVERY_HTTP_CLIENT_NAME);
            _clusterServiceClient = httpClientFactory.CreateClient(Keys.K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME);
            _addressFactory = new KubernetesAddressFactory(discoveryOptions.HealthPath, discoveryOptions.HealthPathLabel, discoveryOptions.HealthPortLabel, discoveryOptions.HealthSchemeLabel);

        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(cancellationToken);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var clusterName = _discoveryOptions.InCluster ? "host" : _discoveryOptions.ClusterHost;
                _logger.LogInformation($"Starting kubernetes service discovery on cluster {clusterName}");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var livenessDbContext = scope.ServiceProvider.GetRequiredService<HealthChecksDb>();

                    try
                    {
                        var services = await _discoveryClient.GetServices(_discoveryOptions.ServicesLabel, _discoveryOptions.Namespaces);
                        foreach (var item in services.Items)
                        {
                            try
                            {
                                var serviceAddress = _addressFactory.CreateAddress(item);

                                if (serviceAddress != null && !IsLivenessRegistered(livenessDbContext, serviceAddress))
                                {
                                    var statusCode = await CallClusterService(serviceAddress);
                                    if (IsValidHealthChecksStatusCode(statusCode))
                                    {
                                        await RegisterDiscoveredLiveness(livenessDbContext, serviceAddress, item.Metadata.Name);
                                        _logger.LogInformation($"Registered discovered liveness on {serviceAddress} with name {item.Metadata.Name}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error discovering service {item.Metadata.Name}. It might not be visible");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred on kubernetes service discovery");
                    }
                }

                await Task.Delay(_discoveryOptions.RefreshTimeOnSeconds * 1000);
            }
        }
        bool IsLivenessRegistered(HealthChecksDb livenessDb, string host)
        {
            return livenessDb.Configurations
                .Any(lc => lc.Uri == host);
        }
        bool IsValidHealthChecksStatusCode(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.ServiceUnavailable;
        }
        async Task<HttpStatusCode> CallClusterService(string host)
        {
            var response = await _clusterServiceClient.GetAsync(host);
            return response.StatusCode;
        }
        Task<int> RegisterDiscoveredLiveness(HealthChecksDb livenessDb, string host, string name)
        {
            livenessDb.Configurations.Add(new HealthCheckConfiguration()
            {
                Name = name,
                Uri = host,
                DiscoveryService = "kubernetes"
            });

            return livenessDb.SaveChangesAsync();
        }
    }
}
