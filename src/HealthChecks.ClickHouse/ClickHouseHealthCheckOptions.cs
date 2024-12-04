using ClickHouse.Client.ADO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ClickHouse;

/// <summary>
/// Options for <see cref="ClickHouseHealthCheck"/>.
/// </summary>
public class ClickHouseHealthCheckOptions
{
    internal ClickHouseHealthCheckOptions()
    {
        // This ctor is internal on purpose: those who want to use ClickHouseHealthCheckOptions
        // need to specify either ConnectionString or a Func<ClickHouseConnection> when creating it.
        // Making the ConnectionString and Func<ClickHouseConnection> setters internal
        // allows us to ensure that the customers don't try to mix both concepts.
        // By encapsulating all of that, we ensure that all instances of this type are valid.
    }

    /// <summary>
    /// Creates an instance of <see cref="ClickHouseHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionString">The ClickHouse connection string to be used.</param>
    public ClickHouseHealthCheckOptions(string connectionString)
    {
        ConnectionString = Guard.ThrowIfNull(connectionString, throwOnEmptyString: true);
    }

    /// <summary>
    /// Creates an instance of <see cref="ClickHouseHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionFactory">The ClickHouse connection factory to be used.</param>
    public ClickHouseHealthCheckOptions(Func<ClickHouseConnection> connectionFactory)
    {
        ConnectionFactory = Guard.ThrowIfNull(connectionFactory);
    }

    /// <summary>
    /// The ClickHouse connection string to be used.
    /// </summary>
    public string? ConnectionString { get; internal set; }

    /// <summary>
    /// The ClickHouse connection factory to be used.
    /// </summary>
    public Func<ClickHouseConnection>? ConnectionFactory { get; set; }

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = ClickHouseHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<ClickHouseConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
