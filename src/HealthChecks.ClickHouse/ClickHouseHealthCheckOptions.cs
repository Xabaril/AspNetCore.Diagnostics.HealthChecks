using ClickHouse.Client.ADO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ClickHouse;

/// <summary>
/// Options for <see cref="ClickHouseHealthCheck"/>.
/// </summary>
public class ClickHouseHealthCheckOptions
{
    /// <summary>
    /// Creates an instance of <see cref="ClickHouseHealthCheckOptions"/>.
    /// </summary>
    /// <param name="connectionFactory">The ClickHouse connection factory to be used.</param>
    public ClickHouseHealthCheckOptions(Func<ClickHouseConnection> connectionFactory)
    {
        ConnectionFactory = Guard.ThrowIfNull(connectionFactory);
    }

    /// <summary>
    /// The ClickHouse connection factory to be used.
    /// </summary>
    public Func<ClickHouseConnection> ConnectionFactory { get; }

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = ClickHouseHealthCheckBuilderExtensions.HEALTH_QUERY;

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}
