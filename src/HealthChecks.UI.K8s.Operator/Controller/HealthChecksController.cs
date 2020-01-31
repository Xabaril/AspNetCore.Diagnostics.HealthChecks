using System;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Handlers;
using k8s;

namespace HealthChecks.UI.K8s.Operator.Controller
{
    class HealthChecksController : IHealthChecksController
    {
        private readonly IKubernetes _client;
        private readonly DeploymentHandler _deploymentHandler;
        private readonly ServiceHandler _serviceHandler;

        public HealthChecksController(
            IKubernetes client,
            DeploymentHandler deploymentHandler,
            ServiceHandler serviceHandler)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _deploymentHandler = deploymentHandler ?? throw new ArgumentNullException(nameof(deploymentHandler));
            _serviceHandler = serviceHandler ?? throw new ArgumentNullException(nameof(serviceHandler));
        }

        public async Task<DeploymentResult> DeployAsync(HealthCheckResource resource)
        {
            Console.WriteLine($"Creating deployment for hc resource - namespace {resource.Metadata.NamespaceProperty}");

            var deployment = await _deploymentHandler.GetOrCreateAsync(resource);
            
            Console.WriteLine($"Creating service  for hc resource - namespace {resource.Metadata.NamespaceProperty}");
            
            var service = await _serviceHandler.GetOrCreateAsync(resource);
            
            return DeploymentResult.Create(deployment, service);
        }

        public async Task DeleteDeploymentAsync(HealthCheckResource resource)
        {
            Console.WriteLine($"Deleting healthchecks deployment {resource.Spec.Name}");
            await _deploymentHandler.Delete(resource);
            await _serviceHandler.Delete(resource);
        }
    }
}