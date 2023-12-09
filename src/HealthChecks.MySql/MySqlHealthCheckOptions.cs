using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace HealthChecks.MySql;

/// <summary>
/// Options for <see cref="MySqlHealthCheck"/>.
/// </summary>
public class MySqlHealthCheckOptions
{
    /// <summary>
    /// Creates an instance of <see cref="MySqlHealthCheckOptions"/>.
    /// </summary>
    /// <param name="dataSource">The <see cref="MySqlDataSource" /> to be used.</param>
    /// <remarks>
    /// Depending on how the <see cref="MySqlDataSource" /> was configured, the connections it hands out may be pooled.
    /// That is why it should be the exact same <see cref="MySqlDataSource" /> that is used by other parts of your app.
    /// </remarks>
    public MySqlHealthCheckOptions(MySqlDataSource dataSource)
    {
        DataSource = Guard.ThrowIfNull(dataSource);
    }

    /// <summary>
    /// Creates an instance of <see cref="MySqlHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionString">The MySQL connection string to be used.</param>
    /// <remarks>
    /// <see cref="MySqlDataSource"/> supports additional configuration beyond the connection string, such as logging and naming pools for diagnostics.
    /// To specify a data source, use <see cref=" MySqlDataSourceBuilder"/> and the <see cref="MySqlHealthCheckOptions(MySqlDataSource)"/> constructor.
    /// </remarks>
    public MySqlHealthCheckOptions(string connectionString)
    {
        ConnectionString = Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);
    }

    /// <summary>
    /// The MySQL data source to be used.
    /// </summary>
    /// <remarks>
    /// Depending on how the <see cref="MySqlDataSource" /> was configured, the connections it hands out may be pooled.
    /// That is why it should be the exact same <see cref="MySqlDataSource" /> that is used by other parts of your app.
    /// </remarks>
    public MySqlDataSource? DataSource { get; }

    /// <summary>
    /// The MySQL connection string to be used, if <see cref="DataSource"/> isn't set.
    /// </summary>
    /// <remarks>
    /// <see cref="MySqlDataSource"/> supports additional configuration beyond the connection string, such as logging and naming pools for diagnostics.
    /// To specify a data source, use <see cref=" MySqlDataSourceBuilder"/> and the <see cref="MySqlHealthCheckOptions(MySqlDataSource)"/> constructor.
    /// </remarks>
    public string? ConnectionString { get; }

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
