using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Discovery.K8S.Extensions;
using k8s;
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
        private readonly IKubernetes _discoveryClient;
        private readonly HttpClient _clusterServiceClient;
        private readonly KubernetesAddressFactory _addressFactory;

        private Task _executingTask;

        public KubernetesDiscoveryHostedService(
            IServiceProvider serviceProvider,
            KubernetesDiscoverySettings discoveryOptions,
            IHttpClientFactory httpClientFactory,
            IKubernetes discoveryClient,
            ILogger<KubernetesDiscoveryHostedService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _discoveryOptions = discoveryOptions ?? throw new ArgumentNullException(nameof(discoveryOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _discoveryClient = discoveryClient ?? throw new ArgumentNullException(nameof(discoveryClient));
            _clusterServiceClient = httpClientFactory?.CreateClient(Keys.K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME) ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _addressFactory = new KubernetesAddressFactory(discoveryOptions);

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
                _logger.LogInformation("Starting kubernetes service discovery");

                using var scope = _serviceProvider.CreateScope();

                var livenessDbContext = scope.ServiceProvider.GetRequiredService<HealthChecksDb>();

                try
                {
                    var services = await _discoveryClient.GetServices(_discoveryOptions.ServicesLabel, _discoveryOptions.Namespaces, cancellationToken);

                    foreach (var item in services)
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
                                    _logger.LogInformation("Registered discovered liveness service {ServiceName} in namespace {ServiceNamespace} with address {ServiceAddress}", item.Metadata.Name, item.Metadata.NamespaceProperty, serviceAddress);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error discovering service {ServiceName} in namespace {ServiceNamespace}. It might not be visible", item.Metadata.Name, item.Metadata.NamespaceProperty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred on kubernetes service discovery");
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
