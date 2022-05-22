using System;

namespace HealthChecks.UI.Data
{
    public class HealthCheckFailureNotification
    {
        public int Id { get; set; }

        public string HealthCheckName { get; set; } = null!;

        public DateTime LastNotified { get; set; }

        public bool IsUpAndRunning { get; set; }
    }
}
