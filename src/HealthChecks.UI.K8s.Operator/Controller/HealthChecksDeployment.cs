using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator;
using k8s;
using k8s.Models;

public class HealthChecksDeployment {
    
    public static V1Deployment Create(HealthCheckResource resource) {
          
          var metadata = new V1ObjectMeta
            {
                Labels = new Dictionary<string,string>
                {
                    ["resourceId"] = resource.Metadata.Uid,
                    ["app"] = resource.Spec.Name
                },
                Name = $"{resource.Spec.Name}-deploy",
                NamespaceProperty = resource.Metadata.NamespaceProperty
            };

            var spec = new V1DeploymentSpec
            {
                Selector = new V1LabelSelector {
                    MatchLabels = new Dictionary<string,string> {
                        ["app"] = resource.Spec.Name
                    }
                },
                Replicas = 1,
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta {
                        Labels = new Dictionary<string,string> {
                            ["app"] = resource.Spec.Name
                        }
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>
                        {
                            new V1Container
                            {
                                Name = Constants.PodName,
                                Image = Constants.DockerImage,
                                Ports = new List<V1ContainerPort>
                                {
                                    new V1ContainerPort(80)
                                },
                                Env = new List<V1EnvVar>
                                {
                                    new V1EnvVar("ui_path", resource.Spec.UiPath ?? Constants.UIDefaultPath)
                                }
                            }
                        }
                    }
                }
            };

            return new V1Deployment(metadata: metadata, spec: spec);
    }
    public static async Task<bool> Exists(IKubernetes client, HealthCheckResource resource) {

        var deployment  = await client.ListNamespacedDeploymentAsync(resource.Metadata.NamespaceProperty, labelSelector: $"resourceId={resource.Metadata.Uid}");
        return deployment.Items.Any();
    }
}