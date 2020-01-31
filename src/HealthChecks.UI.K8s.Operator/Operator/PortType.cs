namespace HealthChecks.UI.K8s.Operator
{
    public class PortType
    {
        public const string LoadBalancer = "LoadBalancer";
        public const string ClusterIP = "ClusterIP";
        public const string NodePort = "NodePort";
    }
}