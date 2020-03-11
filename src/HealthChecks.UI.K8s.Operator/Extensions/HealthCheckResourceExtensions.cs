using k8s.Models;

namespace HealthChecks.UI.K8s.Operator.Extensions
{
    public static class HealthCheckResourceExtensions
    {
        public static V1OwnerReference CreateOwnerReference(this HealthCheckResource resource)
        {
            return new V1OwnerReference
            {
                Name = resource.Spec.Name,
                ApiVersion = resource.ApiVersion,
                Uid = resource.Metadata.Uid,
                Kind = resource.Kind,
                Controller = true
            };
        }
    }
}
