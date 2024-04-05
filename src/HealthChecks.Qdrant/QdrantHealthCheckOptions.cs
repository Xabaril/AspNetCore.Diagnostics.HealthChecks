using Qdrant.Client;

namespace HealthChecks.Qdrant;

/// <summary>
/// Options for <see cref="QdrantHealthCheck"/>.
/// </summary>
public class QdrantHealthCheckOptions
{
    /// <summary>
    /// A <see cref="QdrantClient"/> instance to be used for the connection.
    /// </summary>
    public QdrantClient? Client { get; set; }

    /// <summary>
    /// An Uri representing a connection string for Qdrant.
    /// </summary>
    /// <remarks>
    /// Needs to be a valid gRPC endpoint URI.
    /// </remarks>
    public Uri? ConnectionUri { get; set; }

    /// <summary>
    /// The ApiKey to be used for the connection.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Timeout setting for connection attempts.
    /// </summary>
    public TimeSpan RequestedConnectionTimeout { get; set; } = default;
}
