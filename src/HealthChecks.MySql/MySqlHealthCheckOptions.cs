using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace HealthChecks.MySql;

/// <summary>
/// Options for <see cref="MySqlHealthCheck"/>.
/// </summary>
public class MySqlHealthCheckOptions
{
    /// <summary>
    /// The MySQL Server connection string to be used.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = MySqlHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<MySqlConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
