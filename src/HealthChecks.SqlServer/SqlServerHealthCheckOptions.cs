using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SqlServer;

/// <summary>
/// Options for <see cref="SqlServerHealthCheck"/>.
/// </summary>
public class SqlServerHealthCheckOptions
{
    /// <summary>
    /// The Sql Server connection string to be used.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = SqlServerHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<SqlConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
