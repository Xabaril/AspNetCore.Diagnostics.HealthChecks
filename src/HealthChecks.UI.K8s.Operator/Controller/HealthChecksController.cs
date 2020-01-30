using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator.Controller
{
    class HealthChecksController : IHealthChecksController
    {
        private readonly IKubernetes _client;

        public HealthChecksController(IKubernetes client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task DeployAsync(HealthCheckResource resource)
        {

            var deploymentExists = await HealthChecksDeployment.Exists(_client, resource);

            if (!deploymentExists)
            {

                Console.WriteLine($"Creating deployment for hc resource: {resource.Metadata.NamespaceProperty}");

                var deployment = HealthChecksDeployment.Create(resource);

                try
                {
                    await _client.CreateNamespacedDeploymentWithHttpMessagesAsync(deployment, resource.Metadata.NamespaceProperty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating deployment: {ex.Message}");
                }

            }

            var serviceExists = await HealthChecksService.Exists(_client, resource);

            if (!serviceExists)
            {
                Console.WriteLine($"Creating service for hc resource: {resource.Spec.Name}");
                try
                {
                    var service = HealthChecksService.Create(resource);
                    await _client.CreateNamespacedServiceAsync(service, resource.Metadata.NamespaceProperty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating service for hc resource {resource.Spec.Name}: {ex.Message}");
                }
            }
        }

        public async Task DeleteDeploymentAsync(HealthCheckResource resource)
        {
            var deploymentExists = await HealthChecksDeployment.Exists(_client, resource);

            if (deploymentExists)
            {
                try {
                    await _client.DeleteNamespacedDeploymentAsync($"{resource.Spec.Name}-deploy", resource.Metadata.NamespaceProperty);
                }
                catch(Exception ex) {
                   Console.WriteLine($"Error deleting deployment for hc resource {resource.Spec.Name}: {ex.Message}");
                }
                
            }

            var serviceExists = await HealthChecksService.Exists(_client, resource);

            if (serviceExists)
            {
                try {
                    await _client.DeleteNamespacedServiceAsync($"{resource.Metadata.Name}-svc", resource.Metadata.NamespaceProperty);
                }
                catch(Exception ex) {
                       Console.WriteLine($"Error deleting service for hc resource {resource.Spec.Name}: {ex.Message}");
                }                
            }
        }
    }
}