using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.Solr
{
    public class SolrOptions
    {
        public string Uri { get; private set; }
        public string Core { get; private set; }
       
        public SolrOptions UseServer(string uri, string core)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Core = core ?? throw new ArgumentNullException(nameof(core));

            return this;
        }
    }
}