using System.Collections.Generic;

namespace HealthChecks.UI.Configuration
{
    public class Settings
    {
        internal List<HealthCheckSetting> HealthChecks { get; set; } = new List<HealthCheckSetting>();
        internal List<WebHookNotification> Webhooks { get; set; } = new List<WebHookNotification>();
        internal int EvaluationTimeInSeconds { get; set; } = 10;
        internal int MinimumSecondsBetweenFailureNotifications { get; set; } = 60 * 10;
        internal string HealthCheckDatabaseConnectionString { get; set; }

        public Settings AddHealthCheckEndpoint(string name, string uri)
        {
            HealthChecks.Add(new HealthCheckSetting
            {
                Name = name,
                Uri = uri
            });

            return this;
        }
        
        public Settings AddWebhookNotification(string name, string uri, string payload, string restorePayload = "")
        {
            Webhooks.Add(new WebHookNotification
            {
                Name = name,
                Uri = uri,
                Payload = payload,
                RestoredPayload = restorePayload
            });
            return this;
        }

        public Settings SetEvaluationTimeInSeconds(int seconds)
        {
            EvaluationTimeInSeconds = seconds;
            return this;
        }
        
        public Settings SetMinimumSecondsBetweenFailureNotifications(int seconds)
        {
            MinimumSecondsBetweenFailureNotifications = seconds;
            return this;
        }

        public Settings SetHealthCheckDatabaseConnectionString(string connectionString)
        {
            HealthCheckDatabaseConnectionString = connectionString;
            return this;
        }
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
