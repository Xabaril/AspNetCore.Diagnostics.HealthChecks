using Neo4j.Driver;
using Neo4jClient;

namespace HealthChecks.Neo4jClient;

/// <summary>
/// Options for <see cref="Neo4jClientHealthCheck"/>.
/// </summary>
public class Neo4jClientHealthCheckOptions
{
    /// <summary>
    /// Client for connecting to a database.
    /// </summary>
    public IGraphClient? GraphClient { get; set; }

    /// <summary>
    /// Host that will be used to connect to the database.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Username that will be used to connect to the database using the <see cref="IGraphClient"/>.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password that will be used to connect to the database using the <see cref="IGraphClient"/>.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Realm that will be used to connect to the database using the <see cref="IGraphClient"/>.
    /// </summary>
    public string? Realm { get; set; }

    /// <summary>
    /// Sets the encryption level for connecting to the database
    /// </summary>
    public EncryptionLevel? EncryptionLevel { get; set; }

    /// <summary>
    /// Sets the encryption level for connecting to the database
    /// </summary>
    public bool SerializeNullValues { get; set; } = false;

    /// <summary>
    /// Sets the encryption level for connecting to the database
    /// </summary>
    public bool UseDriverDataTypes { get; set; } = false;

    /// <summary>
    /// Creates instance of <see cref="Neo4jClientHealthCheckOptions"/>.
    /// </summary>
    /// <param name="graphClient">The client for connecting to the database.</param>
    public Neo4jClientHealthCheckOptions(IGraphClient graphClient)
    {
        GraphClient = Guard.ThrowIfNull(graphClient);
    }

    /// <summary>
    /// Creates instance of <see cref="Neo4jClientHealthCheckOptions"/>.
    /// </summary>
    /// <param name="host">Host that will be used to connect to the database.</param>
    /// <param name="username">Username that will be used to connect to the database.</param>
    /// <param name="password">Password that will be used to connect to the database.</param>
    /// <param name="realm">realm that will be used to connect to the database.</param>
    public Neo4jClientHealthCheckOptions(string? host, string? username, string? password, string? realm)
    {
        Host = Guard.ThrowIfNull(host, true);
        Username = Guard.ThrowIfNull(username, true);
        Password = Guard.ThrowIfNull(password, true);
        Realm = realm;
    }
}
