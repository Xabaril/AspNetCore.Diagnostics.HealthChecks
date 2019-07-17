namespace HealthChecks.UI.Core.Discovery.K8S
{
    public class KubernetesApiEndpoints
    {
        public const string ServicesV1 = "api/v1/services";
        public const string NamespacedServicesV1 = "api/v1/namespaces/{0}/services";
    }
}
