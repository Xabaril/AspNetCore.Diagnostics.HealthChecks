using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator.Handlers
{
    public class DeploymentHandler
    {
        private readonly IKubernetes _client;
        private readonly ILogger<K8sOperator> _logger;

        public DeploymentHandler(IKubernetes client, ILogger<K8sOperator> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<V1Deployment> Get(HealthCheckResource resource)
        {
            var deploymentList = await _client.ListNamespacedDeploymentAsync(resource.Metadata.NamespaceProperty,
                labelSelector: $"resourceId={resource.Metadata.Uid}");
            return deploymentList.Items.FirstOrDefault();
        }

        public async Task<V1Deployment> GetOrCreateAsync(HealthCheckResource resource)
        {
            var deployment = await Get(resource);
            if (deployment != null) return deployment;

            try
            {
                var deploymentResource = Build(resource);
                var response =
                    await _client.CreateNamespacedDeploymentWithHttpMessagesAsync(deploymentResource,
                        resource.Metadata.NamespaceProperty);
                deployment = response.Body;

                _logger.LogInformation("Deployment {deployment} has been created", deployment.Metadata.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating deployment: {error}", ex.Message);
            }

            return deployment;

        }

        public async Task Delete(HealthCheckResource resource)
        {
            try
            {
                await _client.DeleteNamespacedDeploymentAsync($"{resource.Spec.Name}-deploy",
                    resource.Metadata.NamespaceProperty);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting deployment for hc resource: {name} - err: {error}", resource.Spec.Name, ex.Message);
            }
        }

        public V1Deployment Build(HealthCheckResource resource)
        {

            var metadata = new V1ObjectMeta
            {
                Labels = new Dictionary<string, string>
                {
                    ["resourceId"] = resource.Metadata.Uid,
                    ["app"] = resource.Spec.Name
                },
                Name = $"{resource.Spec.Name}-deploy",
                NamespaceProperty = resource.Metadata.NamespaceProperty
            };

            var spec = new V1DeploymentSpec
            {
                Selector = new V1LabelSelector
                {
                    MatchLabels = new Dictionary<string, string>
                    {
                        ["app"] = resource.Spec.Name
                    }
                },
                Replicas = 1,
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string>
                        {
                            ["app"] = resource.Spec.Name
                        }
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                ImagePullPolicy = "Never",
                                Name = Constants.PodName,
                                Image = Constants.DockerImage,
                                Ports = new List<V1ContainerPort>
                                {
                                    new V1ContainerPort(80)
                                },
                                Env = new List<V1EnvVar>
                                {
                                    new V1EnvVar("ui_path", resource.Spec.UiPath ?? Constants.UIDefaultPath),
                                    new V1EnvVar("enable_push_endpoint", "true"),
                                    new V1EnvVar("push_endpoint_secret", valueFrom: new V1EnvVarSource(secretKeyRef: new V1SecretKeySelector("key", $"{resource.Spec.Name}-secret")))
                                }
                            }
                        }
                    }
                }
            };

            return new V1Deployment(metadata: metadata, spec: spec);
        }
    }
}