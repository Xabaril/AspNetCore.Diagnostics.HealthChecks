using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Controller.Controller
{
    internal interface IHealthChecksController
    {
        Task DeployAsync(HealthCheckResource resource);
        Task DeleteDeploymentAsync(HealthCheckResource resource);
    }
}
