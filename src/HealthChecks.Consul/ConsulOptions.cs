namespace HealthChecks.Consul
{
    /// <summary>
    /// Options for <see cref="ConsulHealthCheck"/>.
    /// </summary>
    public class ConsulOptions
    {
        public string HostName { get; set; } = null!;

        public int Port { get; set; }

        public bool RequireHttps { get; set; }

        public bool RequireBasicAuthentication { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }
    }
}
