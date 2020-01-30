namespace HealthChecks.UI.K8s.Operator
{
    internal enum PortType
    {
        LoadBalancer,
        NodePort,
        ClusterIP
    }
}