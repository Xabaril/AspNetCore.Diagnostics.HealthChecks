namespace HealthChecks.UI.Core.Notifications
{
    internal interface IHealthCheckFailureNotifier
    {
        Task NotifyDown(string name, UIHealthReport report);
        Task NotifyWakeUp(string name);
    }
}
