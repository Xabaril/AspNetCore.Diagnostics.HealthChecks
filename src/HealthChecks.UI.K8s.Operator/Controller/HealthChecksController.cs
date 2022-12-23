using HealthChecks.UI.K8s.Operator.Extensions;
using HealthChecks.UI.K8s.Operator.Handlers;
using k8s;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator.Controller
{
    internal class HealthChecksController : IHealthChecksController
    {
        private readonly IKubernetes _client;
        private readonly DeploymentHandler _deploymentHandler;
        private readonly ServiceHandler _serviceHandler;
        private readonly SecretHandler _secretHandler;
        private readonly ConfigMaphandler _configMapHandler;
        private readonly ILogger<K8sOperator> _logger;

        public HealthChecksController(
            IKubernetes client,
            DeploymentHandler deploymentHandler,
            ServiceHandler serviceHandler,
            SecretHandler secretHandler,
            ConfigMaphandler configMapHandler,
            ILogger<K8sOperator> logger)
        {
            _client = Guard.ThrowIfNull(client);
            _deploymentHandler = Guard.ThrowIfNull(deploymentHandler);
            _serviceHandler = Guard.ThrowIfNull(serviceHandler);
            _secretHandler = Guard.ThrowIfNull(secretHandler);
            _configMapHandler = Guard.ThrowIfNull(configMapHandler);
            _logger = Guard.ThrowIfNull(logger);
        }

        public async Task<DeploymentResult> DeployAsync(HealthCheckResource resource)
        {
            _logger.LogInformation("Creating secret for hc resource - namespace {namespace}", resource.Metadata.NamespaceProperty);

            var secret = await _secretHandler.GetOrCreateAsync(resource);

            if (resource.HasBrandingConfigured())
            {
                _logger.LogInformation("Creating configmap for hc resource - namespace {namespace}", resource.Metadata.NamespaceProperty);
                await _configMapHandler.GetOrCreateAsync(resource);
            }

            _logger.LogInformation("Creating deployment for hc resource - namespace {namespace}", resource.Metadata.NamespaceProperty);

            var deployment = await _deploymentHandler.GetOrCreateAsync(resource);

            _logger.LogInformation("Creating service for hc resource - namespace {namespace}", resource.Metadata.NamespaceProperty);

            var service = await _serviceHandler.GetOrCreateAsync(resource);

            return DeploymentResult.Create(deployment, service, secret);
        }

        public ValueTask DeleteDeploymentAsync(HealthCheckResource resource)
        {
            _logger.LogInformation("Deleting healthchecks deployment {name} and all referenced resources", resource.Spec.Name);
            return new ValueTask();
        }
    }
}
