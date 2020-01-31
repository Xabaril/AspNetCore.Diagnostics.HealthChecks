using k8s;
using k8s.Models;
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

        public SecretHandler(IKubernetes client)
        {
            _client = client;
        }

        public async Task<V1Secret> GetOrCreate(HealthCheckResource resource)
        {
            var secret = await Get(resource);
            if (secret != null) return secret;

            try
            {
                var secretResource = Build(resource);
                secret = await _client.CreateNamespacedSecretAsync(secretResource,
                              resource.Metadata.NamespaceProperty);

                Console.WriteLine($"Secret {secret.Metadata.Name} has been created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Secret: {ex.Message}");
            }

            return secret;
        }

        public async Task<V1Secret> Get(HealthCheckResource resource)
        {
            var secrets = await _client.ListNamespacedSecretAsync(resource.Metadata.NamespaceProperty,
                labelSelector: $"resourceId={resource.Metadata.Uid}");

            return secrets.Items.FirstOrDefault();
        }

        public async Task Delete(HealthCheckResource resource)
        {
            try
            {
                await _client.DeleteNamespacedSecretAsync($"{resource.Spec.Name}-secret", resource.Metadata.NamespaceProperty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting secret for hc resource {resource.Spec.Name}: {ex.Message}");
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
                    Labels = new Dictionary<string, string>
                    {
                        ["resourceId"] = resource.Metadata.Uid,
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
