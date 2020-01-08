namespace HealthChecks.UI.K8s.Controller
{
    public class HealthCheckResourceSpec
    {
        public string Image { get; set; }
        public string ListeningPort { get; set; }
        public string UiPath { get; set; }
    }    
}
