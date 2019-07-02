namespace HealthChecks.UI.Configuration
{
    public class Options
    {
        public string UIPath { get; set; } = "/healthchecks-ui";
        public string ApiPath { get; set; } = "/healthchecks-api";
        public bool UseRelativeApiPath = true;
        public string WebhookPath { get; set; } = "/healthchecks-webhooks";
        public bool UseRelativeWebhookPath = true;
        public string ResourcesPath { get; set; } = "/ui/resources";
        public bool UseRelativeResourcesPath = true;
    }
}
