namespace HealthChecks.Hazelcast;

public class HazelcastHealthCheckOptions
{
    public string? Server { get; set; }
    public int Port { get; set; }
    public List<string> ClusterNames { get; set; } = new List<string>();
}
