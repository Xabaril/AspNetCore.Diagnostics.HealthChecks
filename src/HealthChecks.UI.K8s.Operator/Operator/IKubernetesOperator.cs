using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.K8s.Operator
{
    internal interface IKubernetesOperator : IDisposable
    {
        Task RunAsync(CancellationToken token);
    }
}
