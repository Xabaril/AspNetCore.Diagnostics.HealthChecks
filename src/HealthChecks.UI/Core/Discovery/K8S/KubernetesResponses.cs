using Newtonsoft.Json;
using System.Collections.Generic;

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
        public Spec Spec { get; set; }
    }
    internal class Metadata
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Uid { get; set; }
    }
    internal class LoadBalancer
    {     
        public Ingress[] Ingress { get; set; }
    }
    internal class Status
    {     
        public LoadBalancer LoadBalancer { get; set; }
    }
    internal class Ingress
    {        
        public string Ip { get; set; }
        public string HostName { get; set; }
    }
    internal class Spec
    {
        public List<Port> Ports { get; set; }
        [JsonProperty("type")]
        public PortType PortType { get; set; }
        public string ClusterIP { get; set; }
    }
    internal class Port
    {
        public string Protocol { get; set; }
        [JsonProperty("Port")]
        public int PortNumber { get; set; }
        public int NodePort { get; set; }
        public string TargetPort { get; set; }
    }
}
