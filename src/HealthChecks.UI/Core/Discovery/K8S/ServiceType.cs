namespace HealthChecks.UI.Core.Discovery.K8S;

internal static class ServiceType
{
    public const string LOAD_BALANCER = "LoadBalancer";
    public const string NODE_PORT = "NodePort";
    public const string CLUSTER_IP = "ClusterIP";
    public const string EXTERNAL_NAME = "ExternalName";
}
