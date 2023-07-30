using System.Net;
using HealthChecks.UI.Core.Discovery.K8S.Extensions;
using HealthChecks.UI.Data;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core.Discovery.K8S
{
    internal sealed class KubernetesDiscoveryHostedService : IHostedService, IDisposable
    {
        private readonly KubernetesDiscoverySettings _discoveryOptions;
        private readonly ILogger<KubernetesDiscoveryHostedService> _logger;
        private readonly IHostApplicationLifetime _hostLifetime;
        private readonly IServiceProvider _serviceProvider;
        private IKubernetes? _discoveryClient;
        private readonly HttpClient _clusterServiceClient;
        private readonly KubernetesAddressFactory _addressFactory;

        private Task? _executingTask;
        private bool _disposed;

        public KubernetesDiscoveryHostedService(
            IServiceProvider serviceProvider,
            IOptions<KubernetesDiscoverySettings> discoveryOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<KubernetesDiscoveryHostedService> logger,
            IHostApplicationLifetime hostLifetime)
        {
            _serviceProvider = Guard.ThrowIfNull(serviceProvider);
            _discoveryOptions = Guard.ThrowIfNull(discoveryOptions?.Value);
            _logger = Guard.ThrowIfNull(logger);
            _hostLifetime = Guard.ThrowIfNull(hostLifetime);
            _clusterServiceClient = Guard.ThrowIfNull(httpClientFactory?.CreateClient(Keys.K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME));
            _addressFactory = new KubernetesAddressFactory(_discoveryOptions);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(cancellationToken);

            return _executingTask.IsCompleted
                ? _executingTask
                : Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _discoveryClient?.Dispose();
            _disposed = true;
        }

        private Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _hostLifetime.ApplicationStarted.Register(async () =>
            {
                if (_discoveryOptions.Enabled)
                {
                    try
                    {
#pragma warning disable IDISP003 // Dispose previous before re-assigning
                        _discoveryClient = InitializeKubernetesClient();
#pragma warning restore IDISP003 // Dispose previous before re-assigning
                        await StartK8sServiceAsync(cancellationToken);
                    }
                    catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        // We are halting, task cancellation is expected.
                    }
                }

            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask != null)
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private async Task StartK8sServiceAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting kubernetes service discovery");

                using var scope = _serviceProvider.CreateScope();

                var livenessDbContext = scope.ServiceProvider.GetRequiredService<HealthChecksDb>();

                try
                {
                    var services = await _discoveryClient!.GetServicesAsync(_discoveryOptions.ServicesLabel, _discoveryOptions.Namespaces, cancellationToken);

                    if (services != null)
                    {
                        foreach (var item in services.Items)
                        {
                            try
                            {
                                var serviceAddress = _addressFactory.CreateAddress(item);

                                if (serviceAddress != null && !IsLivenessRegistered(livenessDbContext, serviceAddress))
                                {
                                    var statusCode = await CallClusterServiceAsync(serviceAddress);
                                    if (IsValidHealthChecksStatusCode(statusCode))
                                    {
                                        await RegisterDiscoveredLiveness(livenessDbContext, serviceAddress, item.Metadata.Name);
                                        _logger.LogInformation($"Registered discovered liveness on {serviceAddress} with name {item.Metadata.Name}");
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                _logger.LogError($"Error discovering service {item.Metadata.Name}. It might not be visible");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred on kubernetes service discovery");
                }

                await Task.Delay(_discoveryOptions.RefreshTimeInSeconds * 1000);
            }
        }

        private static bool IsLivenessRegistered(HealthChecksDb livenessDb, string host)
        {
            return livenessDb.Configurations
                .Any(lc => lc.Uri == host);
        }

        private static bool IsValidHealthChecksStatusCode(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.ServiceUnavailable;
        }

        private async Task<HttpStatusCode> CallClusterServiceAsync(string host)
        {
            using var response = await _clusterServiceClient.GetAsync(host, HttpCompletionOption.ResponseHeadersRead);
            return response.StatusCode;
        }

        private Task<int> RegisterDiscoveredLiveness(HealthChecksDb livenessDb, string host, string name)
        {
            livenessDb.Configurations.Add(new HealthCheckConfiguration
            {
                Name = name,
                Uri = host,
                DiscoveryService = "kubernetes"
            });

            return livenessDb.SaveChangesAsync();
        }

        private IKubernetes InitializeKubernetesClient()
        {
            KubernetesClientConfiguration kubernetesConfig;

            if (!string.IsNullOrEmpty(_discoveryOptions.ClusterHost) && !string.IsNullOrEmpty(_discoveryOptions.Token))
            {
                kubernetesConfig = new KubernetesClientConfiguration
                {
                    Host = _discoveryOptions.ClusterHost,
                    AccessToken = _discoveryOptions.Token,
                    // Some cloud services like Azure AKS use self-signed certificates not valid for httpclient.
                    // With this method we allow invalid certificates
                    SkipTlsVerify = true
                };
            }
            else if (KubernetesClientConfiguration.IsInCluster())
            {
                kubernetesConfig = KubernetesClientConfiguration.InClusterConfig();
            }
            else
            {
                kubernetesConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            }

            return new Kubernetes(kubernetesConfig);
        }
    }
}
