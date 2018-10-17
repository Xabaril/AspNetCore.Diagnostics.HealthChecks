using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.Notifications
{
    interface IHealthCheckFailureNotifier
    {
        Task NotifyDown(string name, HealthReport report);

        Task NotifyWakeUp(string name);
    }
}
