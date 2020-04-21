using HealthChecks.UI.K8s.Operator.Crd;
using System.Collections.Generic;

namespace HealthChecks.UI.K8s.Operator
{
    public class HealthCheckResourceSpec
    {
        public string Name { get; set; }
        public string PortNumber { get; set; }
        public string ServiceType { get; set; }
        public string UiPath { get; set; }
        public string ServicesLabel { get; set; }
        public string HealthChecksPath { get; set; }
        public string HealthChecksScheme { get; set; }
        public string Image { get; set; }
        public string ImagePullPolicy { get; set; }
        public string StylesheetContent { get; set; }
        public List<NameValueObject> ServiceAnnotations { get; set; } = new List<NameValueObject>();
        public List<NameValueObject> DeploymentAnnotations { get; set; } = new List<NameValueObject>();
    }
}
