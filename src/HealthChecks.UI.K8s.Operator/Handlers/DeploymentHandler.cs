using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Extensions;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using static HealthChecks.UI.K8s.Operator.Constants;

namespace HealthChecks.UI.K8s.Operator.Handlers;

internal class DeploymentHandler
{
    private readonly IKubernetes _client;
    private readonly ILogger<K8sOperator> _logger;
    private readonly OperatorDiagnostics _operatorDiagnostics;

    public DeploymentHandler(IKubernetes client, ILogger<K8sOperator> logger, OperatorDiagnostics operatorDiagnostics)
    {
        _client = Guard.ThrowIfNull(client);
        _logger = Guard.ThrowIfNull(logger);
        _operatorDiagnostics = Guard.ThrowIfNull(operatorDiagnostics);
    }

    public Task<V1Deployment?> Get(HealthCheckResource resource)
    {
        return _client.ListNamespacedOwnedDeploymentAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);
    }

    public async Task<V1Deployment> GetOrCreateAsync(HealthCheckResource resource)
    {
        var deployment = await Get(resource);
        if (deployment != null)
            return deployment;

        try
        {
            var deploymentResource = Build(resource);
            var response = await _client.AppsV1.CreateNamespacedDeploymentWithHttpMessagesAsync(deploymentResource, resource.Metadata.NamespaceProperty);
            deployment = response.Body;

            _operatorDiagnostics.DeploymentCreated(deployment.Metadata.Name);
        }
        catch (Exception ex)
        {
            _operatorDiagnostics.DeploymentOperationError(deployment?.Metadata.Name!, Deployment.Operation.ADD, ex.Message);
        }

        return deployment!;
    }

    public async Task DeleteAsync(HealthCheckResource resource)
    {
        try
        {
            await _client.AppsV1.DeleteNamespacedDeploymentAsync($"{resource.Spec.Name}-deploy",
                resource.Metadata.NamespaceProperty);
        }
        catch (Exception ex)
        {
            _operatorDiagnostics.DeploymentOperationError(resource.Spec.Name, Deployment.Operation.DELETE, ex.Message);
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

        var uiContainer = new V1Container
        {
            ImagePullPolicy = resource.Spec.ImagePullPolicy ?? Constants.DEFAULT_PULL_POLICY,
            Name = Constants.POD_NAME,
            Image = resource.Spec.Image ?? Constants.IMAGE_NAME,
            Ports = new List<V1ContainerPort>
                            {
                                new V1ContainerPort { ContainerPort = 80 }
                            },
            Env = new List<V1EnvVar>
                            {
                                new V1EnvVar { Name = "enable_push_endpoint", Value = "true" },
                                new V1EnvVar
                                {
                                    Name = "push_endpoint_secret",
                                    ValueFrom = new V1EnvVarSource
                                    {
                                        SecretKeyRef = new V1SecretKeySelector { Key = "key", Name = $"{resource.Spec.Name}-secret" }
                                    }
                                },
                                new V1EnvVar { Name = "Logging__LogLevel__Default", Value = "Debug" },
                                new V1EnvVar { Name = "Logging__LogLevel__Microsoft", Value = "Warning" },
                                new V1EnvVar { Name = "Logging__LogLevel__System", Value = "Warning" },
                                new V1EnvVar { Name = "Logging__LogLevel__HealthChecks", Value = "Information" }
                            }
        };

        uiContainer.MapCustomUIPaths(resource, _operatorDiagnostics);

        var tolerations = resource.Spec.Tolerations?.Select(toleration => new V1Toleration
        {
            Effect = toleration.Effect,
            Key = toleration.Key,
            OperatorProperty = toleration.Operator,
            TolerationSeconds = toleration.Seconds,
            Value = toleration.Value
        }).ToList() ?? new List<V1Toleration>();

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
                       uiContainer
                    },
                    Tolerations = tolerations
                }
            }
        };

        foreach (var annotation in resource.Spec.DeploymentAnnotations)
        {
            _logger.LogInformation("Adding annotation {Annotation} to ui deployment with value {AnnotationValue}", annotation.Name, annotation.Value);
            metadata.Annotations.Add(annotation.Name, annotation.Value);
        }

        var specification = spec.Template.Spec;
        var container = specification.Containers.First();

        for (int i = 0; i < resource.Spec.Webhooks.Count; i++)
        {
            var webhook = resource.Spec.Webhooks[i];
            _logger.LogInformation("Adding webhook configuration for webhook {Webhook}", webhook.Name);

            container.Env.Add(new V1EnvVar { Name = $"HealthChecksUI__Webhooks__{i}__Name", Value = webhook.Name });
            container.Env.Add(new V1EnvVar { Name = $"HealthChecksUI__Webhooks__{i}__Uri", Value = webhook.Uri });
            container.Env.Add(new V1EnvVar { Name = $"HealthChecksUI__Webhooks__{i}__Payload", Value = webhook.Payload });
            container.Env.Add(new V1EnvVar { Name = $"HealthChecksUI__Webhooks__{i}__RestoredPayload", Value = webhook.RestoredPayload });
        }

        if (resource.HasBrandingConfigured())
        {
            const string volumeName = "healthchecks-volume";

            specification.Volumes ??= new List<V1Volume>();
            container.VolumeMounts ??= new List<V1VolumeMount>();

            specification.Volumes.Add(new V1Volume
            {
                Name = volumeName,
                ConfigMap = new V1ConfigMapVolumeSource { Name = $"{resource.Spec.Name}-config" }
            });

            container.Env.Add(new V1EnvVar { Name = "ui_stylesheet", Value = $"{Constants.STYLES_PATH}/{Constants.STYLE_SHEET_NAME}" });
            container.VolumeMounts.Add(new V1VolumeMount { MountPath = $"/app/{Constants.STYLES_PATH}", Name = volumeName });
        }

        return new V1Deployment { Metadata = metadata, Spec = spec };
    }
}
