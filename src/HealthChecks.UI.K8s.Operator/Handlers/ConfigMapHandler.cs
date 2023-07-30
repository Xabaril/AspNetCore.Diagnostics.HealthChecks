using System.Text;
using HealthChecks.UI.K8s.Operator.Extensions;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator.Handlers;

public class ConfigMaphandler
{
    private readonly IKubernetes _client;
    private readonly ILogger<K8sOperator> _logger;

    public ConfigMaphandler(IKubernetes client, ILogger<K8sOperator> logger)
    {
        _client = Guard.ThrowIfNull(client);
        _logger = Guard.ThrowIfNull(logger);
    }

    public Task<V1ConfigMap?> Get(HealthCheckResource resource)
    {
        return _client.ListNamespacedOwnedConfigMapAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);
    }

    public async Task<V1ConfigMap?> GetOrCreateAsync(HealthCheckResource resource)
    {
        var configMap = await Get(resource);
        if (configMap != null)
            return configMap;

        try
        {
            var configMapResource = Build(resource);
            configMap = await _client.CoreV1.CreateNamespacedConfigMapAsync(configMapResource, resource.Metadata.NamespaceProperty);
            _logger.LogInformation("Config Map {name} has been created", configMap.Metadata.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating config map for hc resource {name} : {message}", resource.Spec.Name, ex.Message);
        }

        return configMap;
    }

    public async Task DeleteAsync(HealthCheckResource resource)
    {
        try
        {
            await _client.CoreV1.DeleteNamespacedConfigMapAsync($"{resource.Spec.Name}-config", resource.Metadata.NamespaceProperty);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting config map for hc resource {name} : {message}", resource.Spec.Name, ex.Message);
        }
    }

    public V1ConfigMap Build(HealthCheckResource resource)
    {
        return new V1ConfigMap
        {
            BinaryData = new Dictionary<string, byte[]>
            {
                [Constants.STYLE_SHEET_NAME] = Encoding.UTF8.GetBytes(resource.Spec.StylesheetContent)
            },
            Metadata = new V1ObjectMeta
            {
                OwnerReferences = new List<V1OwnerReference>
                {
                     resource.CreateOwnerReference(),
                },
                NamespaceProperty = resource.Metadata.NamespaceProperty,
                Name = $"{resource.Spec.Name}-config"
            }
        };
    }
}
