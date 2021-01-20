using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class DnsResolveCountOptions
    {
        internal Dictionary<string, (int min, int? max)> HostRegistrations = new();

        public DnsResolveCountOptions AddHost(string hostName, int minHosts, int? maxHosts)
        {
            if (!HostRegistrations.TryGetValue(hostName, out var _))
            {
                HostRegistrations.Add(hostName, (minHosts, maxHosts));
            }
            return this;
        }
    }
}
