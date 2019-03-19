namespace HealthChecks.Consul
{
    public class ConsulOptions
    {
        public string HostName { get; set; }

        public int Port { get; set; }

        public bool RequireHttps { get; set; }
    }
}
