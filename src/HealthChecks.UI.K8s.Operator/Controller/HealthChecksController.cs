using System;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Handlers;
using k8s;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator.Controller
{
    class HealthChecksController : IHealthChecksController
    {
        private readonly IKubernetes _client;
        private readonly DeploymentHandler _deploymentHandler;
        private readonly ServiceHandler _serviceHandler;
        private readonly SecretHandler _secretHandler;
        private readonly ILogger<K8sOperator> _logger;

        public HealthChecksController(
            IKubernetes client,
            DeploymentHandler deploymentHandler,
            ServiceHandler serviceHandler,
            SecretHandler secretHandler,
            ILogger<K8sOperator> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _deploymentHandler = deploymentHandler ?? throw new ArgumentNullException(nameof(deploymentHandler));
            _serviceHandler = serviceHandler ?? throw new ArgumentNullException(nameof(serviceHandler));
            _secretHandler = secretHandler ?? throw new ArgumentNullException(nameof(secretHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DeploymentResult> DeployAsync(HealthCheckResource resource)
        {
            _logger.LogInformation("Creating secret for hc resource - namespace {namespace}", resource.Metadata.NamespaceProperty);

            var secret = await _secretHandler.GetOrCreate(resource);

            _logger.LogInformation("Creating deployment for hc resource - namespace {namespace}", resource.Metadata.NamespaceProperty);

            var deployment = await _deploymentHandler.GetOrCreateAsync(resource);

            _logger.LogInformation("Creating service for hc resource - namespace {namespace}", resource.Metadata.NamespaceProperty);

            var service = await _serviceHandler.GetOrCreateAsync(resource);
            
            return DeploymentResult.Create(deployment, service, secret);
        }

        public async Task DeleteDeploymentAsync(HealthCheckResource resource)
        {
            _logger.LogInformation("Deleting healthchecks deployment {name}", resource.Spec.Name);

            await _secretHandler.Delete(resource);
            await _deploymentHandler.Delete(resource);
            await _serviceHandler.Delete(resource);
        }
    }
}