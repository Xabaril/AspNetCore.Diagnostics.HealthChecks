namespace HealthChecks.Network;

public class DnsResolveOptions
{
    internal Dictionary<string, DnsRegistration> ConfigureHosts = new();
    internal void AddHost(string host, DnsRegistration registration)
    {
        ConfigureHosts.Add(host, registration);
    }

    public DnsResolveOptions WithCheckAllHosts()
    {
        CheckAllHosts = true;
        return this;
    }

    public bool CheckAllHosts { get; set; }
}
