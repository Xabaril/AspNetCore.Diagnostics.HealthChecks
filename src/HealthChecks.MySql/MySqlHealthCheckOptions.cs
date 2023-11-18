using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace HealthChecks.MySql;

/// <summary>
/// Options for <see cref="MySqlHealthCheck"/>.
/// </summary>
public class MySqlHealthCheckOptions
{
    /// <summary>
    /// The MySQL data source to be used. This is the preferred way to specify the MySQL server to be checked.
    /// </summary>
    public MySqlDataSource? DataSource { get; set; }

    /// <summary>
    /// The MySQL connection string to be used, if <see cref="DataSource"/> isn't set.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string? CommandText { get; set; }

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<MySqlConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
