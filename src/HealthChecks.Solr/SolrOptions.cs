using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.Solr
{
    public class SolrOptions
    {
        public string Uri { get; private set; }
        public string Core { get; private set; }
        public int Timeout { get; private set; }

        public SolrOptions UseServer(string uri, string core, int timeout = 1000)

        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Core = core ?? throw new ArgumentNullException(nameof(core));
            Timeout = timeout;

            return this;
        }
    }
}
