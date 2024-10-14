using HealthChecks.Redis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="RedisHealthCheck"/>.
/// </summary>
public static class RedisHealthCheckBuilderExtensions
{
    private const string NAME = "redis";

    /// <summary>
    /// Add a health check for Redis services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="redisConnectionString">The Redis connection string to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'redis' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddRedis(
        this IHealthChecksBuilder builder,
        string redisConnectionString,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddRedis(_ => redisConnectionString, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Redis services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Redis connection string to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'redis' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddRedis(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);

        return builder.Add(new HealthCheckRegistration(
           name ?? NAME,
           sp => new RedisHealthCheck(connectionStringFactory(sp)),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Redis services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Redis connection string to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'redis' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddRedis(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, CancellationToken, Task<string?>> connectionStringFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);

        return builder.Add(new HealthCheckRegistration(
           name ?? NAME,
           sp => new RedisHealthCheck((ct) => connectionStringFactory(sp, ct)),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Redis services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionMultiplexer">The Redis connection to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'redis' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddRedis(
        this IHealthChecksBuilder builder,
        IConnectionMultiplexer connectionMultiplexer,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddRedis(_ => connectionMultiplexer, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Redis services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionMultiplexerFactory">A factory to build the Redis connection to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'redis' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddRedis(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IConnectionMultiplexer> connectionMultiplexerFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionMultiplexerFactory);

        return builder.Add(new HealthCheckRegistration(
           name ?? NAME,
           sp => new RedisHealthCheck(() => connectionMultiplexerFactory(sp)),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Redis services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionMultiplexerFactory">A factory to build the Redis connection to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'redis' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddRedis(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, CancellationToken, Task<IConnectionMultiplexer>> connectionMultiplexerFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionMultiplexerFactory);

        return builder.Add(new HealthCheckRegistration(
           name ?? NAME,
           sp => new RedisHealthCheck((ct) => connectionMultiplexerFactory(sp, ct)),
           failureStatus,
           tags,
           timeout));
    }
}
