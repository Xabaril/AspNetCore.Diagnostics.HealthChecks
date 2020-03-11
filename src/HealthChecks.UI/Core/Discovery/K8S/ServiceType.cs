namespace HealthChecks.UI.Core.Discovery.K8S
{
    internal static class ServiceType
    {
        public const string LoadBalancer = "LoadBalancer";
        public const string NodePort = "NodePort";
        public const string ClusterIP = "ClusterIP";
        public const string ExternalName = "ExternalName";
    }
}
