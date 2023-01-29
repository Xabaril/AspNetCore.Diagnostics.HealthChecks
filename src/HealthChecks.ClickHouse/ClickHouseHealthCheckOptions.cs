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
    /// The ClickHouse connection string to be used.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

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
