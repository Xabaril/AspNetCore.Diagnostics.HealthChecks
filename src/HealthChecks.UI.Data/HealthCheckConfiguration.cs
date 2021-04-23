namespace HealthChecks.UI.Data
{
    public class HealthCheckConfiguration
    {
        public int Id { get; set; }

        public string Uri { get; set; }

        public string Name { get; set; }

        public string DiscoveryService { get; set; }

        public void Deconstruct(out string uri, out string name)
        {
            uri = Uri;
            name = Name;
        }
    }
}
