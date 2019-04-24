using System.Collections.Generic;

namespace HealthChecks.Network
{
    public class TcpHealthCheckOptions
    {
        internal List<(string host, int port)> ConfiguredHosts = new List<(string host, int port)>();
        public TcpHealthCheckOptions AddHost(string host, int port)
        {
            ConfiguredHosts.Add((host, port));
            return this;
        }
    }
}
