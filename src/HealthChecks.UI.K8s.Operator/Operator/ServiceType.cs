namespace HealthChecks.UI.K8s.Operator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "TODO: rename public API")]
    public class ServiceType
    {
        public const string LoadBalancer = "LoadBalancer";
        public const string ClusterIP = "ClusterIP";
        public const string NodePort = "NodePort";
    }
}
