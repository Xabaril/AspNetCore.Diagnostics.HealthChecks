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
        private readonly SecretHandler _secretHandler;

        public HealthChecksController(
            IKubernetes client,
            DeploymentHandler deploymentHandler,
            ServiceHandler serviceHandler,
            SecretHandler secretHandler)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _deploymentHandler = deploymentHandler ?? throw new ArgumentNullException(nameof(deploymentHandler));
            _serviceHandler = serviceHandler ?? throw new ArgumentNullException(nameof(serviceHandler));
            _secretHandler = secretHandler ?? throw new ArgumentNullException(nameof(secretHandler));
        }

        public async Task<DeploymentResult> DeployAsync(HealthCheckResource resource)
        {
            Console.WriteLine($"Creating secret for hc resource - namespace {resource.Metadata.NamespaceProperty}");

            var secret = await _secretHandler.GetOrCreate(resource);

            Console.WriteLine($"Creating deployment for hc resource - namespace {resource.Metadata.NamespaceProperty}");

            var deployment = await _deploymentHandler.GetOrCreateAsync(resource);
            
            Console.WriteLine($"Creating service  for hc resource - namespace {resource.Metadata.NamespaceProperty}");
            
            var service = await _serviceHandler.GetOrCreateAsync(resource);
            
            return DeploymentResult.Create(deployment, service, secret);
        }

        public async Task DeleteDeploymentAsync(HealthCheckResource resource)
        {
            Console.WriteLine($"Deleting healthchecks deployment {resource.Spec.Name}");
            await _secretHandler.Delete(resource);
            await _deploymentHandler.Delete(resource);
            await _serviceHandler.Delete(resource);
        }
    }
}