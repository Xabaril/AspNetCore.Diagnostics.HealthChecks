namespace HealthChecks.UI.Configuration
{
    public class Options
    {
        public string UIPath { get; set; } = "/healthchecks-ui";

        public string ApiPath { get; set; } = "/healthchecks-api";

        public string WebhookPath { get; set; } = "/healthchecks-webhooks";
    }
}
