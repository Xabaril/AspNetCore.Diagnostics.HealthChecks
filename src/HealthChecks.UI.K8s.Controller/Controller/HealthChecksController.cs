using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Controller.Controller
{
    class HealthChecksController : IHealthChecksController
    {
        public Task DeleteDeploymentAsync(HealthCheckResource resource)
        {
            throw new NotImplementedException();
        }

        public Task DeployAsync(HealthCheckResource resource)
        {
            throw new NotImplementedException();
        }
    }
}
