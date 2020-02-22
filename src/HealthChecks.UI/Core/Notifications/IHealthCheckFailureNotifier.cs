using HealthChecks.UI.Client;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.Notifications
{
    interface IHealthCheckFailureNotifier<T>
    {
        Task NotifyDown(T _db, string name, UIHealthReport report);
        Task NotifyWakeUp(T _db,string name);
    }
}
