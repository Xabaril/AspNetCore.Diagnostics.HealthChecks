using System;

namespace HealthChecks.Network
{
    public class DnsRegistration
    {
        internal string Host { get; }

        internal string[]? Resolutions { get; set; }

        public DnsRegistration(string host)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
        }
    }
}
