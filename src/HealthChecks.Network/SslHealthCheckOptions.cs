using System;
using System.Collections.Generic;

namespace HealthChecks.Network
{
    public class SslHealthCheckOptions
    {
        internal List<(string host, int port, int checkLeftDays)> ConfiguredHosts = new List<(string host, int port, int checkLeftDays)>();
        public SslHealthCheckOptions AddHost(string host, int port = 443, int checkLeftDays = 60)
        {
            ConfiguredHosts.Add((host, port, checkLeftDays));
            return this;
        }
    }
}
