namespace HealthChecks.UI.Core.Discovery.Docker
{
    internal class DockerDiscoveredContainer
    {
        public string Id { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Scheme { get; set; }
        public int? Port { get; set; }
    }
}