using System.Threading.Tasks;

namespace HealthChecks.UI.Core.Notifications
{
    interface IHealthCheckFailureNotifier
    {
        Task NotifyDown(string livenessName, string message);

        Task NotifyWakeUp(string livenessName);
    }
}
