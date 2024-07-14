namespace HealthChecks.Solr;

public class SolrOptions
{
    public string Uri { get; private set; } = null!;

    public string Core { get; private set; } = null!;

    public string? Username { get; private set; }

    public string? Password { get; private set; }

    public TimeSpan Timeout { get; private set; }

    public SolrOptions UseServer(string uri, string core, string? username, string? password, TimeSpan? timeout)
    {
        Uri = Guard.ThrowIfNull(uri);
        Core = Guard.ThrowIfNull(core);
        Username = username;
        Password = password;
        Timeout = timeout ?? TimeSpan.FromMilliseconds(1000);

        return this;
    }
}
