namespace HealthChecks.Gremlin
{
    /// <summary>
    /// Options for <see cref="GremlinHealthCheck"/>.
    /// </summary>
    public class GremlinOptions
    {
        public string Hostname { get; set; } = "localhost";

        public int Port { get; set; } = 8182;

        public bool EnableSsl { get; set; } = true;
    }
}
