using HealthChecks.Qdrant;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Qdrant.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class QdrantHealthCheckBuilderExtensions
{
    private const string NAME = "qdrant";

    /// <summary>
    /// Add a health check for Qdrant services using connection string (grpc).
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="qdrantConnectionString">The Qdrant connection string to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'qdrant' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddQdrant(
        this IHealthChecksBuilder builder,
        string qdrantConnectionString,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddQdrant(new Uri(qdrantConnectionString), name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Qdrant services using connection string (grpc).
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="qdrantConnectionString">The Qdrant connection string to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'qdrant' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddQdrant(
        this IHealthChecksBuilder builder,
        Uri qdrantConnectionString,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new QdrantHealthCheckOptions
        {
            ConnectionUri = qdrantConnectionString
        };

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            new QdrantHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Qdrant services using <see cref="QdrantClient"/> from service provider. At least one must be configured.
    /// </summary>
    /// <remarks>
    /// This method shouldn't be called more than once.
    /// Each subsequent call will create a new connection, which overrides the previous ones.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'qdrant' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddQdrant(
        this IHealthChecksBuilder builder,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        builder.Services.AddSingleton(sp =>
        {
            var client = sp.GetService<QdrantClient>();

            if (client != null)
            {
                return new QdrantHealthCheck(new() { Client = client });
            }
            else
            {
                throw new ArgumentException($"A QdrantClient must be registered with the service provider");
            }
        });

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => sp.GetRequiredService<QdrantHealthCheck>(),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Qdrant services.
    /// </summary>
    /// <remarks>
    /// <paramref name="setup"/> will be called each time the healthcheck route is requested. However
    /// the created <see cref="QdrantClient"/> will be reused.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure the Qdrant setup.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'qdrant' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddQdrant(
        this IHealthChecksBuilder builder,
        Action<QdrantHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new QdrantHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            new QdrantHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Qdrant services.
    /// </summary>
    /// <remarks>
    /// <paramref name="setup"/> will be called each time the healthcheck route is requested. However
    /// the created <see cref="QdrantClient"/> will be reused.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure the Qdrant setup with <see cref="IServiceProvider"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'qdrant' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddQdrant(
        this IHealthChecksBuilder builder,
        Action<IServiceProvider, QdrantHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new QdrantHealthCheckOptions();

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                setup?.Invoke(sp, options);

                return new QdrantHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }
}
