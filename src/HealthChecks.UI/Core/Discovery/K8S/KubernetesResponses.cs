using Newtonsoft.Json;

namespace HealthChecks.UI.Core.Discovery.K8S
{
    internal class ServiceList
    {
        [JsonProperty("items")]
        public Service[] Items { get; set; } = new Service[] { };
    }

    internal class Service
    {     
        public Metadata Metadata { get; set; }
        public Status Status { get; set; }
    }

    internal class Metadata
    {
        
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Uid { get; set; }
    }

    public partial class LoadBalancer
    {     
        public Ingress[] Ingress { get; set; }
    }

    public partial class Status
    {     
        public LoadBalancer LoadBalancer { get; set; }
    }

    public partial class Ingress
    {        
        public string Ip { get; set; }
    }
}
