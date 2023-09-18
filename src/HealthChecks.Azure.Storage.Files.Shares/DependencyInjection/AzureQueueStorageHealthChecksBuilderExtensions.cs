using Azure.Storage.Files.Shares;
using HealthChecks.Azure.Storage.Files.Shares;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureFileShareHealthCheck"/>.
/// </summary>
public static class AzureFileShareStorageHealthChecksBuilderExtensions
{
    private const string HEALTH_CHECK_NAME = "azure_file_share";

    /// <summary>
    /// Add a health check for Azure Files by registering <see cref="AzureFileShareHealthCheck"/> for given <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add <see cref="HealthCheckRegistration"/> to.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="ShareServiceClient" /> instance.
    /// When not provided, <see cref="ShareServiceClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="AzureFileShareHealthCheckOptions"/> used by the health check.
    /// When not provided, defaults are used.
    /// </param>
    /// <param name="healthCheckName">The health check name. Optional. If <c>null</c> the name 'azure_file_share' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureFileShare(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, ShareServiceClient>? clientFactory = default,
        Func<IServiceProvider, AzureFileShareHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = HEALTH_CHECK_NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           string.IsNullOrEmpty(healthCheckName) ? HEALTH_CHECK_NAME : healthCheckName!,
           sp => new AzureFileShareHealthCheck(
                    shareServiceClient: clientFactory?.Invoke(sp) ?? sp.GetRequiredService<ShareServiceClient>(),
                    options: optionsFactory?.Invoke(sp)),
           failureStatus,
           tags,
           timeout));
    }
}
