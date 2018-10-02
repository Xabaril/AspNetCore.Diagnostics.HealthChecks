using System;

namespace HealthChecks.UI.Core.Data
{
    public class HealthCheckFailureNotification
    {
        public int Id { get; set; }

        public string HealthCheckName { get; set; }

        public DateTime LastNotified { get; set; }

        public bool IsUpAndRunning { get; set; }
    }
}
