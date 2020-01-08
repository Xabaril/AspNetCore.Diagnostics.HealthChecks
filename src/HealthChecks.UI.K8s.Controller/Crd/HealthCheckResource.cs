using HealthChecks.UI.K8s.Controller.Crd;
using System;


namespace HealthChecks.UI.K8s.Controller
{
    public class HealthCheckResource : CustomResource<HealthCheckResourceSpec, Object> { }
    
}
