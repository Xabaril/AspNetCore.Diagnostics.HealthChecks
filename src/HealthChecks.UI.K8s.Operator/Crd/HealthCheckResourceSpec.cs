namespace HealthChecks.UI.K8s.Operator
{
    public class HealthCheckResourceSpec
    {
        public string Name { get; set; }
        public string ListeningPort { get; set; }
        public string UiPath { get; set; }
        public string ServicesLabel {get;set;}
        public string ServicesPathLabel { get; set; }
        public string ServicesSchemeLabel { get; set; }
    }    
}
