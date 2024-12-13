using HealthChecks.SendGrid;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="SendGridHealthCheck"/>.
/// </summary>
public static class SendGridHealthCheckExtensions
{
    internal const string NAME = "sendgrid";

    /// <summary>
    /// Add a health check for SendGrid.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sendgrid' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSendGrid(
        this IHealthChecksBuilder builder,
        string apiKey,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddSendGrid(_ => apiKey, name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a health check for SendGrid.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="apiKeyFactory">Factory to resolve the API key.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sendgrid' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSendGrid(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> apiKeyFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        string registrationName = name ?? NAME;

        builder.Services.AddHttpClient(registrationName);

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp =>
            {
                string apiKey = apiKeyFactory(sp);
                return new SendGridHealthCheck(apiKey, sp.GetRequiredService<IHttpClientFactory>());
            },
            failureStatus,
            tags,
            timeout));
    }
}
