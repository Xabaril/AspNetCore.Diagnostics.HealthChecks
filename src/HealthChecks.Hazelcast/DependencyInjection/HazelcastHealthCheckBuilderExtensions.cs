using HealthChecks.Hazelcast;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class HazelcastHealthCheckBuilderExtensions
{
    private const string NAME = "hazelcast";

    /// <summary>
    /// Adds a Hazelcast health check to the health checks builder.
    /// </summary>
    /// <param name="builder">The health checks builder to which the Hazelcast health check is added.</param>
    /// <param name="configureOptions">
    /// A factory function that provides Hazelcast health check options.
    /// The function takes an <see cref="IServiceProvider"/> and returns a <see cref="HazelcastHealthCheckOptions"/>.
    /// </param>
    /// <param name="name">The name of the health check. If null, the default name "hazelcast" is used.</param>
    /// <param name="failureStatus">
    /// The health status to report when the health check fails. If null, the default status is used.
    /// </param>
    /// <param name="tags">A collection of tags that can be used to filter health checks.</param>
    /// <param name="timeout">
    /// An optional timeout for the health check. If the health check takes longer than this timeout, it will be considered a failure.
    /// </param>
    /// <returns>The updated <see cref="IHealthChecksBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configureOptions"/> is null.</exception>
    public static IHealthChecksBuilder AddHazelcast(
      this IHealthChecksBuilder builder,
      Action<HazelcastHealthCheckOptions> configureOptions,
      string? name = default,
      HealthStatus? failureStatus = default,
      IEnumerable<string>? tags = default,
      TimeSpan? timeout = default)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions), "Options cannot be null");

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                var options = new HazelcastHealthCheckOptions();
                configureOptions(options);
                return new HazelcastHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }
}
