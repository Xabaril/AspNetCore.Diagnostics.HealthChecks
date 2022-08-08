using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.ApplicationStatus.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ApplicationStatusHealthCheck"/>.
/// </summary>
public static class ApplicationStatusHealthCheckBuilderExtensions
{
    private const string NAME = "applicationstatus";

    /// <summary>
    /// Add a health check for Application Status.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="lifetime">Represents an object for intercepting application lifetime events.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'applicationstatus' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddApplicationStatus(
        this IHealthChecksBuilder builder,
        IHostApplicationLifetime lifetime,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        if (lifetime == null)
        {
            throw new ArgumentNullException(nameof(IHostApplicationLifetime));
        }

        builder.Services
            .TryAddSingleton(sp => new ApplicationStatusHealthCheck(lifetime));

        return AddApplicationStatusCheck(builder, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Application Status.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'applicationstatus' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddApplicationStatus(
        this IHealthChecksBuilder builder,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        builder.Services
            .TryAddSingleton(sp => new ApplicationStatusHealthCheck(sp.GetRequiredService<IHostApplicationLifetime>()));

        return AddApplicationStatusCheck(builder, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for Application Status from DI container.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'applicationstatus' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    private static IHealthChecksBuilder AddApplicationStatusCheck(
        this IHealthChecksBuilder builder,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => sp.GetRequiredService<ApplicationStatusHealthCheck>(),
            failureStatus,
            tags,
            timeout));
    }
}
