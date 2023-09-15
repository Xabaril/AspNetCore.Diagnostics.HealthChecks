using Azure.Storage.Blobs;
using HealthChecks.AzureStorage;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureBlobStorageHealthCheck"/>.
/// </summary>
public static class AzureBlobStorageHealthChecksBuilderExtensions
{
    private const string HEALTH_CHECK_NAME = "azure_blob_storage";

    /// <summary>
    /// Add a health check for Azure Blob Storage by registering <see cref="AzureBlobStorageHealthCheck"/> for given <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add <see cref="HealthCheckRegistration"/> to.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="BlobServiceClient" /> instance.
    /// When not provided, <see cref="BlobServiceClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="AzureBlobStorageHealthCheckOptions"/> used by the health check.
    /// When not provided, defaults are used.
    /// </param>
    /// <param name="healthCheckName">The health check name. Optional. If <c>null</c> the name 'azure_blob_storage' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureBlobStorage(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, BlobServiceClient>? clientFactory = default,
        Func<IServiceProvider, AzureBlobStorageHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = HEALTH_CHECK_NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           string.IsNullOrEmpty(healthCheckName) ? HEALTH_CHECK_NAME : healthCheckName!,
           sp => new AzureBlobStorageHealthCheck(
                    blobServiceClient: clientFactory?.Invoke(sp) ?? sp.GetRequiredService<BlobServiceClient>(),
                    options: optionsFactory?.Invoke(sp)),
           failureStatus,
           tags,
           timeout));
    }
}
