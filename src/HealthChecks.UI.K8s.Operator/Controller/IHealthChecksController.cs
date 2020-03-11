using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator.Controller
{
    internal interface IHealthChecksController
    {
        Task<DeploymentResult> DeployAsync(HealthCheckResource resource);
        Task DeleteDeploymentAsync(HealthCheckResource resource);
    }
}
