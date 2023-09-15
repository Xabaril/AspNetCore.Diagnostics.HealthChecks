namespace HealthChecks.Hazelcast;

/// <summary>
/// Options for <see cref="HazelcastHealthCheck"/>.
/// </summary>
public class HazelcastHealthCheckOptions
{
    /// <summary>
    /// Hazelcast connection host ({ip/DNS}:{port})
    /// </summary>
    public string ConnectionHost { get; set; } = null!;

    /// <summary>
    /// Hazelcast cluster name to connect
    /// </summary>
    public string ClusterName { get; set; } = "dev";

    /// <summary>
    /// Hazelcast connection client name (use to track the healthcheck connection to your Hazelcast)
    /// </summary>
    public string ClientName { get; set; } = null!;

    /// <summary>
    /// Hazelcast client connection timeout
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromMilliseconds(100);
}
