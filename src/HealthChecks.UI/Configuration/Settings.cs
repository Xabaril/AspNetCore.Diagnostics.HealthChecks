using System.Collections.Generic;

namespace HealthChecks.UI.Configuration
{
    public class Settings
    {
        public List<HealthCheckSetting> HealthChecks { get; set; } = new List<HealthCheckSetting>();
        public List<WebHookNotification> Webhooks { get; set; } = new List<WebHookNotification>();
        public int EvaluationTimeOnSeconds { get; set; } = 10;
        public int MinimumSecondsBetweenFailureNotifications { get; set; } = 60 * 10;
        public string HealthCheckDatabaseConnectionString { get; set; }
    }

    public class HealthCheckSetting
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    public class WebHookNotification
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Payload { get; set; }
        public string RestoredPayload { get; set; }
    }
}
