namespace HealthChecks.UI.Core.Data
{
    internal class HealthCheckConfiguration
    {
        public int Id { get; set; }

        public string Uri { get; set; }

        public string Name { get; set; }

        public string DiscoveryService { get; set; }

        public void Deconstruct(out string uri, out string name)
        {
            uri = this.Uri;
            name = this.Name;
        }
    }
}
