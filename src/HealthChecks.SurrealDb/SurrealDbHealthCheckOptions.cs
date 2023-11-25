using Microsoft.Extensions.Diagnostics.HealthChecks;
using SurrealDb.Net;

namespace HealthChecks.SurrealDb;

/// <summary>
/// Options for <see cref="SurrealDbHealthCheck"/>.
/// </summary>
public class SurrealDbHealthCheckOptions
{
    /// <summary>
    /// The Sql Server connection string to be used.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<ISurrealDbClient>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
