using HealthChecks.UI.Client;
using System;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.Notifications
{
    interface IHealthCheckFailureNotifier
        :IDisposable
    {
        Task NotifyDown(string name, UIHealthReport report);
        Task NotifyWakeUp(string name);
    }
}
