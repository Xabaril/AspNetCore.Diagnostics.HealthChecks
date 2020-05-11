using HealthChecks.UI.K8s.Operator.Extensions;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator.Handlers
{
    public class SecretHandler
    {
        private readonly IKubernetes _client;
        private readonly ILogger<K8sOperator> _logger;

        public SecretHandler(IKubernetes client, ILogger<K8sOperator> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<V1Secret> GetOrCreateAsync(HealthCheckResource resource)
        {
            var secret = await Get(resource);
            if (secret != null) return secret;

            try
            {
                var secretResource = Build(resource);
                secret = await _client.CreateNamespacedSecretAsync(secretResource,
                              resource.Metadata.NamespaceProperty);

                _logger.LogInformation("Secret {name} has been created", secret.Metadata.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating Secret: {message}", ex.Message);
            }

            return secret;
        }

        public Task<V1Secret> Get(HealthCheckResource resource)
        {
            return _client.ListNamespacedOwnedSecretAsync(resource.Metadata.NamespaceProperty, resource.Metadata.Uid);
        }

        public async Task Delete(HealthCheckResource resource)
        {
            try
            {
                await _client.DeleteNamespacedSecretAsync($"{resource.Spec.Name}-secret", resource.Metadata.NamespaceProperty);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting secret for hc resource {name} : {message}", resource.Spec.Name, ex.Message);
            }
        }

        public V1Secret Build(HealthCheckResource resource)
        {
            return new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = $"{resource.Spec.Name}-secret",
                    NamespaceProperty = resource.Metadata.NamespaceProperty,
                    OwnerReferences = new List<V1OwnerReference> {
                        resource.CreateOwnerReference()
                    },
                    Labels = new Dictionary<string, string>
                    {
                        ["app"] = resource.Spec.Name
                    }
                },
                Data = new Dictionary<string, byte[]>
                {
                    ["key"] = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())
                }
            };
        }

    }
}
