using System;

namespace HealthChecks.Solr
{
    public class SolrOptions
    {
        public string Uri { get; private set; }
        public string Core { get; private set; }
        public TimeSpan Timeout { get; private set; }

        public SolrOptions UseServer(string uri, string core, TimeSpan? timeout)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Core = core ?? throw new ArgumentNullException(nameof(core));
            Timeout = timeout ?? TimeSpan.FromMilliseconds(1000);

            return this;
        }
    }
}
