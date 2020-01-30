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

        public Task DeleteDeploymentAsync(HealthCheckResource resource)
        {
            return Task.CompletedTask;
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
                    Console.WriteLine(ex.Message);
                }

            }

            var serviceExists = await HealthChecksService.Exists(_client, resource);

            if (!serviceExists)
            {
                Console.WriteLine($"Creating service for hc resource: {resource.Metadata.NamespaceProperty}");
                  try
                {
                    var service = HealthChecksService.Create(resource);
                    await _client.CreateNamespacedServiceAsync(service, resource.Metadata.NamespaceProperty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

        }
    }
}