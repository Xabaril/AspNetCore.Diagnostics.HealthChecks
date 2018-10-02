using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core
{
    interface ILivenessRunner
    {
        Task Run(CancellationToken cancellationToken);
    }
}
