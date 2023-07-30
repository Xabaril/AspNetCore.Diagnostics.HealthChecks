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
                                new V1ContainerPort(80)
                            },
            Env = new List<V1EnvVar>
                            {
                                new V1EnvVar("enable_push_endpoint", "true"),
                                new V1EnvVar("push_endpoint_secret", valueFrom: new V1EnvVarSource(secretKeyRef: new V1SecretKeySelector("key", $"{resource.Spec.Name}-secret"))),
                                new V1EnvVar("Logging__LogLevel__Default", "Debug"),
                                new V1EnvVar("Logging__LogLevel__Microsoft", "Warning"),
                                new V1EnvVar("Logging__LogLevel__System", "Warning"),
                                new V1EnvVar("Logging__LogLevel__HealthChecks", "Information")
                            }
        };

        uiContainer.MapCustomUIPaths(resource, _operatorDiagnostics);

        var tolerations = resource.Spec.Tolerations?.Select(toleration => new V1Toleration(toleration.Effect,
            toleration.Key, toleration.Operator, toleration.Seconds, toleration.Value)).ToList() ?? new List<V1Toleration>();

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

            container.Env.Add(new V1EnvVar($"HealthChecksUI__Webhooks__{i}__Name", webhook.Name));
            container.Env.Add(new V1EnvVar($"HealthChecksUI__Webhooks__{i}__Uri", webhook.Uri));
            container.Env.Add(new V1EnvVar($"HealthChecksUI__Webhooks__{i}__Payload", webhook.Payload));
            container.Env.Add(new V1EnvVar($"HealthChecksUI__Webhooks__{i}__RestoredPayload", webhook.RestoredPayload));
        }

        if (resource.HasBrandingConfigured())
        {
            const string volumeName = "healthchecks-volume";

            specification.Volumes ??= new List<V1Volume>();
            container.VolumeMounts ??= new List<V1VolumeMount>();

            specification.Volumes.Add(new V1Volume(name: volumeName,
                configMap: new V1ConfigMapVolumeSource(name: $"{resource.Spec.Name}-config")));

            container.Env.Add(new V1EnvVar("ui_stylesheet", $"{Constants.STYLES_PATH}/{Constants.STYLE_SHEET_NAME}"));
            container.VolumeMounts.Add(new V1VolumeMount($"/app/{Constants.STYLES_PATH}", volumeName));
        }

        return new V1Deployment(metadata: metadata, spec: spec);
    }
}
