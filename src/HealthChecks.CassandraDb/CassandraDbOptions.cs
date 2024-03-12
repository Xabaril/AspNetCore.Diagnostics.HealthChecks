using Cassandra;

namespace HealthChecks.CassandraDb;

/// <summary>
/// Options for CassandraHealthCheck.
/// </summary>
public class CassandraDbOptions
{
    public string ContactPoint { get; set; } = null!;
    public string Keyspace { get; set; } = null!;
    public string Query { get; set; } = "SELECT now() FROM system.local;";
    public Action<Builder> ConfigureClusterBuilder { get; set; } = null!;
}
