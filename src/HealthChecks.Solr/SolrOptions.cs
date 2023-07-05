namespace HealthChecks.Solr;

public class SolrOptions
{
    public string Uri { get; private set; } = null!;

    public string Core { get; private set; } = null!;

    public TimeSpan Timeout { get; private set; }

    public SolrOptions UseServer(string uri, string core, TimeSpan? timeout)
    {
        Uri = Guard.ThrowIfNull(uri);
        Core = Guard.ThrowIfNull(core);
        Timeout = timeout ?? TimeSpan.FromMilliseconds(1000);

        return this;
    }
}
