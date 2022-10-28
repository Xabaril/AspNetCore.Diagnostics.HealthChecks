namespace HealthChecks.Network
{
    public class DnsRegistration
    {
        internal string Host { get; }

        internal string[]? Resolutions { get; set; }

        public DnsRegistration(string host)
        {
            Host = Guard.ThrowIfNull(host);
        }
    }
}
