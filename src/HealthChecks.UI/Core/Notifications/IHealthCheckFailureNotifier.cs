namespace HealthChecks.UI.Core.Notifications;

public interface IHealthCheckFailureNotifier
{
    Task NotifyDown(string name, UIHealthReport report);
    Task NotifyWakeUp(string name);
}
