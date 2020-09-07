using System;

namespace HealthChecks.Gremlin
{
    public class GremlinOptions
    {
        public string Hostname { get; set; }
        public int Port { get; set; } = 8182;
        public bool EnableSsl { get; set; } = true;
    }
}
