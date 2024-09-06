using Cassandra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CassandraDb.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="CassandraDbHealthCheck"/>.
/// </summary>
public static class CassandraDbHealthCheckBuilderExtensions
{
    private const string NAME = "cassandra";

    /// <summary>
    /// Add a health check for Cassandra databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="contactPoint">The Cassandra contact point to be used.</param>
    /// <param name="keyspace">The Cassandra keyspace to be used.</param>
    /// <param name="query">The query to be executed. Default is 'SELECT now() FROM system.local;'.</param>
    /// <param name="configureClusterBuilder">An action to allow additional Cassandra specific configuration. It cannot be null.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'cassandra' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCassandra(
        this IHealthChecksBuilder builder,
        string contactPoint,
        string keyspace,
        string query = "SELECT now() FROM system.local;",
        Action<Builder>? configureClusterBuilder = null, // Note: The caller must ensure this is not null
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        // Define a default configuration action if none is provided
        var defaultConfigurationAction = new Action<Builder>(b =>
            // Default minimal configuration
            b.AddContactPoint(contactPoint).Build());

        var options = new CassandraDbOptions
        {
            ContactPoint = contactPoint,
            Keyspace = keyspace,
            Query = query,
            ConfigureClusterBuilder = configureClusterBuilder ?? defaultConfigurationAction // Use provided configuration or default
        };

        return builder.AddCassandra(options, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Cassandra databases using <see cref="CassandraDbOptions"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for the Cassandra health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'cassandra' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCassandra(
        this IHealthChecksBuilder builder,
        CassandraDbOptions options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        // Ensure options.ConfigureClusterBuilder is never null
        if (options.ConfigureClusterBuilder == null)
        {
            throw new ArgumentNullException(nameof(options.ConfigureClusterBuilder), "ConfigureClusterBuilder action cannot be null.");
        }

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new CassandraDbHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
