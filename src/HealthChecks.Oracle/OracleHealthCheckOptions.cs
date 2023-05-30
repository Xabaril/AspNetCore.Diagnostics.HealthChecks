using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Oracle.ManagedDataAccess.Client;

namespace HealthChecks.Oracle;

/// <summary>
/// Options for <see cref="OracleHealthCheck"/>.
/// </summary>
public class OracleHealthCheckOptions
{
    /// <summary>
    /// The Oracle connection string to be used.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Optional credential to use when connecting to the database.
    /// </summary>
    public OracleCredential? Credential { get; set; }

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = OracleHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<OracleConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
