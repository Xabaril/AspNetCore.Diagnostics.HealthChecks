using System.Collections.Generic;
using System.Net;

namespace HealthChecks.Network
{
    public class FtpHealthCheckOptions
    {
        internal Dictionary<string, (string host, bool createFile, NetworkCredential credentials)> Hosts { get; } = new Dictionary<string, (string, bool, NetworkCredential)>();

        public FtpHealthCheckOptions AddHost(string host, bool createFile = false, NetworkCredential credentials = null)
        {
            Hosts.Add(host, (host, createFile, credentials));

            return this;
        }
    }
}