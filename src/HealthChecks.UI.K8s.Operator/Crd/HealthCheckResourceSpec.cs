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
    }
}
