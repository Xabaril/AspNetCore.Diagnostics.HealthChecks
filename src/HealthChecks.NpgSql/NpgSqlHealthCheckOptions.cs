using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace HealthChecks.NpgSql;

/// <summary>
/// Options for <see cref="NpgSqlHealthCheck"/>.
/// </summary>
public class NpgSqlHealthCheckOptions
{
    internal NpgSqlHealthCheckOptions()
    {
        // This ctor is internal on purpose: those who want to use NpgSqlHealthCheckOptions
        // need to specify either ConnectionString or DataSource when creating it.
        // Making the ConnectionString and DataSource setters internal
        // allows us to ensure that the customers don't try to mix both concepts.
        // By encapsulating all of that, we ensure that all instances of this type are valid.
    }

    /// <summary>
    /// Creates an instance of <see cref="NpgSqlHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string to be used.</param>
    /// <remarks>
    /// <see cref="NpgsqlDataSource"/> supports additional configuration beyond the connection string,
    /// such as logging, advanced authentication options, type mapping management, and more.
    /// To further configure a data source, use <see cref=" NpgsqlDataSourceBuilder"/> and
    /// the <see cref="NpgSqlHealthCheckOptions(NpgsqlDataSource)"/> constructor.
    /// </remarks>
    public NpgSqlHealthCheckOptions(string connectionString)
    {
        ConnectionString = Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);
    }

    /// <summary>
    /// Creates an instance of <see cref="NpgSqlHealthCheckOptions"/>.
    /// </summary>
    /// <param name="dataSource">The Postgres <see cref="NpgsqlDataSource" /> to be used.</param>
    /// <remarks>
    /// Depending on how the <see cref="NpgsqlDataSource" /> was configured, the connections it hands out may be pooled.
    /// That is why it should be the exact same <see cref="NpgsqlDataSource" /> that is used by other parts of your app.
    /// </remarks>
    public NpgSqlHealthCheckOptions(NpgsqlDataSource dataSource)
    {
        DataSource = Guard.ThrowIfNull(dataSource);
    }

    /// <summary>
    /// The Postgres connection string to be used.
    /// </summary>
    /// <remarks>
    /// <see cref="NpgsqlDataSource"/> supports additional configuration beyond the connection string,
    /// such as logging, advanced authentication options, type mapping management, and more.
    /// To further configure a data source, use <see cref=" NpgsqlDataSourceBuilder"/> and the <see cref="NpgSqlHealthCheckOptions(NpgsqlDataSource)"/> constructor.
    /// </remarks>
    public string? ConnectionString { get; internal set; }

    /// <summary>
    /// The Postgres <see cref="NpgsqlDataSource" /> to be used.
    /// </summary>
    /// <remarks>
    /// Depending on how the <see cref="NpgsqlDataSource" /> was configured, the connections it hands out may be pooled.
    /// That is why it should be the exact same <see cref="NpgsqlDataSource" /> that is used by other parts of your app.
    /// </remarks>
    public NpgsqlDataSource? DataSource { get; internal set; }

    internal bool TriedToResolveFromDI;

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = NpgSqlHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<NpgsqlConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
