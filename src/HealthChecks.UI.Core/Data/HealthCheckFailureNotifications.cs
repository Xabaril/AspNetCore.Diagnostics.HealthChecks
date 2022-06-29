namespace HealthChecks.UI.Core.Data
{
    public class HealthCheckFailureNotification
    {
        public int Id { get; set; }

        public string HealthCheckName { get; set; } = null!;

        public DateTime LastNotified { get; set; }

        public bool IsUpAndRunning { get; set; }
    }
}
