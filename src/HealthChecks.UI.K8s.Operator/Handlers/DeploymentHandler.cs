using HealthChecks.UI.K8s.Operator.Extensions;
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

        public Task<V1Deployment> Get(HealthCheckResource resource)
        {
            return _client.ListNamespacedOwnedDeploymentAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);
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
                OwnerReferences = new List<V1OwnerReference> {
                    resource.CreateOwnerReference()
                },
                Annotations = new Dictionary<string, string>(),
                Labels = new Dictionary<string, string>
                {
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
                        },
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                ImagePullPolicy = resource.Spec.ImagePullPolicy ?? Constants.DefaultPullPolicy,
                                Name = Constants.PodName,
                                Image = resource.Spec.Image ?? Constants.ImageName,
                                Ports = new List<V1ContainerPort>
                                {
                                    new V1ContainerPort(80)
                                },
                                Env = new List<V1EnvVar>
                                {
                                    new V1EnvVar("ui_path", resource.Spec.UiPath ?? Constants.DefaultUIPath),
                                    new V1EnvVar("enable_push_endpoint", "true"),
                                    new V1EnvVar("push_endpoint_secret", valueFrom: new V1EnvVarSource(secretKeyRef: new V1SecretKeySelector("key", $"{resource.Spec.Name}-secret"))),
                                    new V1EnvVar("Logging__LogLevel__Default", "Debug"),
                                    new V1EnvVar("Logging__LogLevel__Microsoft", "Warning"),
                                    new V1EnvVar("Logging__LogLevel__System", "Warning"),
                                    new V1EnvVar("Logging__LogLevel__HealthChecks", "Information")
                                }
                            }
                        }
                    }
                }
            };

            foreach (var annotation in resource.Spec.DeploymentAnnotations)
            {
                _logger.LogInformation("Adding annotation {Annotation} to ui deployment with value {AnnotationValue}", annotation.Name, annotation.Value);
                metadata.Annotations.Add(annotation.Name, annotation.Value);
            }

            if (resource.HasBrandingConfigured())
            {
                var specification = spec.Template.Spec;
                var container = specification.Containers.First();
                var volumeName = "healthchecks-volume";

                if (specification.Volumes == null) specification.Volumes = new List<V1Volume>();
                if (container.VolumeMounts == null) container.VolumeMounts = new List<V1VolumeMount>();

                specification.Volumes.Add(new V1Volume(name: volumeName,
                    configMap: new V1ConfigMapVolumeSource(name: $"{resource.Spec.Name}-config")));

                container.Env.Add(new V1EnvVar("ui_stylesheet", $"{Constants.StylesPath}/{Constants.StyleSheetName}"));
                container.VolumeMounts.Add(new V1VolumeMount($"/app/{Constants.StylesPath}", volumeName));
            }

            return new V1Deployment(metadata: metadata, spec: spec);
        }
    }
}