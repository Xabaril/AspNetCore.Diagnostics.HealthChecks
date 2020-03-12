using System;
using HealthChecks.UI.K8s.Operator.Crd;

namespace HealthChecks.UI.K8s.Operator
{
    public class HealthCheckResource : CustomResource<HealthCheckResourceSpec, Object> { }
    
}
