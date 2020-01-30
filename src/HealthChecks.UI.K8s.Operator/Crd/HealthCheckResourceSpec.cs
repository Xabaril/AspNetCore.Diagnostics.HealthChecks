namespace HealthChecks.UI.K8s.Operator
{
    public class HealthCheckResourceSpec
    {
        public string Name { get; set; }
        public string ListeningPort { get; set; }
        public string UiPath { get; set; }
    }    
}
