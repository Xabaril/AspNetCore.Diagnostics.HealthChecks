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
        // this ctor is internal on purpose:
        // those who want to use DataSource need to use the extension methods
        // that take care of creating the right thing 
    }

    /// <summary>
    /// Creates an instance of <see cref="NpgSqlHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string to be used.</param>
    public NpgSqlHealthCheckOptions(string connectionString)
    {
        ConnectionString = Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);
    }

    /// <summary>
    /// The Postgres connection string to be used.
    /// Use <see cref="DataSource"/> property for advanced configuration.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The Postgres data source to be used.
    /// </summary>
    internal NpgsqlDataSource? DataSource { get; set; }

    internal bool TriedToResolveFromDI { get; set; }

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
