using DotPulsar.Abstractions;
using HealthChecks.Pulsar;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="PulsarHealthCheck"/>.
/// </summary>
public static class PulsarHealthCheckBuilderExtensions
{
    private const string NAME = "pulsar";
    internal const string DEFAULT_TOPIC = "non-persistent://public/default/healthchecks-topic";

    private static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Add a health check for Pulsar cluster.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="topic">The topic name to produce Pulsar messages on.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPulsar(
        this IHealthChecksBuilder builder,
        string topic = DEFAULT_TOPIC,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default
    ) => AddPulsar(
        builder,
        sp => sp.GetRequiredService<IPulsarClient>(),
        topic: topic,
        name: name,
        failureStatus: failureStatus,
        tags: tags,
        timeout: timeout ?? DEFAULT_TIMEOUT);

    /// <summary>
    /// Add a health check for Pulsar cluster.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">The Pulsar client factory.</param>
    /// <param name="topic">The topic name to produce Pulsar messages on.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPulsar(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IPulsarClient> clientFactory,
        string topic = DEFAULT_TOPIC,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default
    )
    {
        builder.Services.TryAddSingleton(sp => new PulsarHealthCheck(clientFactory(sp), new PulsarHealthCheckOptions
        {
            Topic = topic
        }));

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => sp.GetRequiredService<PulsarHealthCheck>(),
            failureStatus,
            tags,
            timeout ?? DEFAULT_TIMEOUT));
    }

    /// <summary>
    /// Add a health check for Pulsar cluster.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options to configure Pulsar health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPulsar(
        this IHealthChecksBuilder builder,
        PulsarHealthCheckOptions options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default
    ) => AddPulsar(
        builder,
        options,
        sp => sp.GetRequiredService<IPulsarClient>(),
        name: name,
        failureStatus: failureStatus,
        tags: tags,
        timeout: timeout ?? DEFAULT_TIMEOUT);

    /// <summary>
    /// Add a health check for Pulsar cluster.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options to configure Pulsar health check.</param>
    /// <param name="clientFactory">The Pulsar client factory.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPulsar(
        this IHealthChecksBuilder builder,
        PulsarHealthCheckOptions options,
        Func<IServiceProvider, IPulsarClient> clientFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default
    )
    {
        builder.Services.TryAddSingleton(sp => new PulsarHealthCheck(clientFactory(sp), options));

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => sp.GetRequiredService<PulsarHealthCheck>(),
            failureStatus,
            tags,
            timeout ?? DEFAULT_TIMEOUT));
    }
}
