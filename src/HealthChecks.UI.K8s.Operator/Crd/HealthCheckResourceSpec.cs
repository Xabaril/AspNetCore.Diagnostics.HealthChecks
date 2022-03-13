using HealthChecks.UI.K8s.Operator.Crd;

namespace HealthChecks.UI.K8s.Operator
{
    public class HealthCheckResourceSpec
    {
        public string Name { get; set; } = null!;
        public string Scope { get; set; } = null!;
        public string? PortNumber { get; set; }
        public string? ServiceType { get; set; }
        public string? UiPath { get; set; }
        public string? UiApiPath { get; set; }
        public string? UiResourcesPath { get; set; }
        public string? UiWebhooksPath { get; set; }
        public bool? UiNoRelativePaths { get; set; }
        public string ServicesLabel { get; set; }
        public string HealthChecksPath { get; set; }
        public string HealthChecksScheme { get; set; }
        public string Image { get; set; }
        public string ImagePullPolicy { get; set; }
        public string StylesheetContent { get; set; }
        public List<NameValueObject> ServiceAnnotations { get; set; } = new List<NameValueObject>();
        public List<NameValueObject> DeploymentAnnotations { get; set; } = new List<NameValueObject>();
        public List<WebHookObject> Webhooks { get; set; } = new List<WebHookObject>();
        public List<TolerationObject> Tolerations { get; set; } = new List<TolerationObject>();
    }
}
