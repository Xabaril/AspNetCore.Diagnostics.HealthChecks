using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using HealthChecks.AzureStorage;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureBlobStorageHealthCheck"/>,
/// <see cref="AzureFileShareHealthCheck"/>, <see cref="AzureQueueStorageHealthCheck"/>.
/// </summary>
public static class AzureStorageHealthCheckBuilderExtensions
{
    private const string AZUREBLOB_NAME = "azureblob";
    private const string AZUREFILESHARE_NAME = "azurefileshare";
    private const string AZUREQUEUE_NAME = "azurequeue";

    /// <summary>
    /// Add a health check for Azure Blob Storage.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure Storage connection string to be used. </param>
    /// <param name="containerName">The Azure Storage container name to check if exist. Optional, If <see langword="null"/> then container name check is not executed. </param>
    /// <param name="clientOptions">Provide the client configuration options to connect with Azure Storage.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azureblob' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureBlobStorage(
        this IHealthChecksBuilder builder,
        string connectionString,
        string? containerName = default,
        BlobClientOptions? clientOptions = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREBLOB_NAME,
           sp => new AzureBlobStorageHealthCheck(connectionString, containerName, clientOptions),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure Blob Storage.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="blobServiceUri">The Azure Storage Blob service Uri like <see href="https://myaccount.blob.core.windows.net"/>. </param>
    /// <param name="credential">The TokenCredentail to use, you can use Azure.Identity with DefaultAzureCredential or other kind of TokenCredential,you can read more on <see href="https://github.com/Azure/azure-sdk-for-net/blob/Azure.Identity_1.2.2/sdk/identity/Azure.Identity/README.md"/>. </param>
    /// <param name="containerName">The Azure Storage container name to check if exist. Optional, If <see langword="null"/> then container name check is not executed. </param>
    /// <param name="clientOptions">Provide the client configuration options to connect with Azure Storage.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azureblob' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureBlobStorage(
        this IHealthChecksBuilder builder,
        Uri blobServiceUri,
        TokenCredential credential,
        string? containerName = default,
        BlobClientOptions? clientOptions = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREBLOB_NAME,
           sp => new AzureBlobStorageHealthCheck(blobServiceUri, credential, containerName, clientOptions),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure Blob Storage.
    /// </summary>
    /// <remarks>
    /// A <see cref="BlobServiceClient"/> service must be registered in the service container.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azureblob' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureBlobStorage(
        this IHealthChecksBuilder builder,
        Action<AzureBlobStorageHealthCheckOptions>? configureOptions = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREBLOB_NAME,
           sp =>
           {
               var options = new AzureBlobStorageHealthCheckOptions();
               configureOptions?.Invoke(options);
               return new AzureBlobStorageHealthCheck(sp.GetRequiredService<BlobServiceClient>(), options);
           },
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure Blob Storage.
    /// </summary>
    /// <remarks>
    /// A <see cref="BlobServiceClient"/> service must be registered in the service container.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azureblob' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureBlobStorage(
        this IHealthChecksBuilder builder,
        Action<IServiceProvider, AzureBlobStorageHealthCheckOptions>? configureOptions = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREBLOB_NAME,
           sp =>
           {
               var options = new AzureBlobStorageHealthCheckOptions();
               configureOptions?.Invoke(sp, options);
               return new AzureBlobStorageHealthCheck(sp.GetRequiredService<BlobServiceClient>(), options);
           },
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for an Azure file share.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure Storage connection string to be used.</param>
    /// <param name="shareName">The name of the Azure file share to check if exist.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azurefileshare' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureFileShare(
        this IHealthChecksBuilder builder,
        string connectionString,
        string? shareName = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREFILESHARE_NAME,
           sp => new AzureFileShareHealthCheck(connectionString, shareName),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for an Azure file share.
    /// </summary>
    /// <remarks>
    /// A <see cref="ShareServiceClient"/> service must be registered in the service container.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azurefileshare' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureFileShare(
        this IHealthChecksBuilder builder,
        Action<AzureFileShareHealthCheckOptions>? configureOptions = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREFILESHARE_NAME,
           sp =>
           {
               var options = new AzureFileShareHealthCheckOptions();
               configureOptions?.Invoke(options);
               return new AzureFileShareHealthCheck(sp.GetRequiredService<ShareServiceClient>(), options);
           },
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for an Azure file share.
    /// </summary>
    /// <remarks>
    /// A <see cref="ShareServiceClient"/> service must be registered in the service container.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azurefileshare' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureFileShare(
        this IHealthChecksBuilder builder,
        Action<IServiceProvider, AzureFileShareHealthCheckOptions>? configureOptions = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREFILESHARE_NAME,
           sp =>
           {
               var options = new AzureFileShareHealthCheckOptions();
               configureOptions?.Invoke(sp, options);
               return new AzureFileShareHealthCheck(sp.GetRequiredService<ShareServiceClient>(), options);
           },
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure Queue Storage.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure Storage connection string to be used. </param>
    /// <param name="queueName">The Azure Storage queue name to check if exist. Optional.If <see langword="null"/> then queue name check is not executed. </param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureQueueStorage(
        this IHealthChecksBuilder builder,
        string connectionString,
        string? queueName = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREQUEUE_NAME,
           sp => new AzureQueueStorageHealthCheck(connectionString, queueName),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure Queue Storage.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="queueServiceUri">The Azure Queue service Uri like <see href="https://myaccount.blob.core.windows.net"/>. </param>
    /// <param name="credential">The TokenCredentail to use, you can use Azure.Identity with DefaultAzureCredential or other kind of TokenCredential,you can read more on <see href="https://github.com/Azure/azure-sdk-for-net/blob/Azure.Identity_1.2.2/sdk/identity/Azure.Identity/README.md"/>. </param>
    /// <param name="queueName">The Azure Storage queue name to check if exist. Optional.If <see langword="null"/> then queue name check is not executed. </param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureQueueStorage(
        this IHealthChecksBuilder builder,
        Uri queueServiceUri,
        TokenCredential credential,
        string? queueName = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREQUEUE_NAME,
           sp => new AzureQueueStorageHealthCheck(queueServiceUri, credential, queueName),
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure Queue Storage.
    /// </summary>
    /// <remarks>
    /// A <see cref="QueueServiceClient"/> service must be registered in the service container.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureQueueStorage(
        this IHealthChecksBuilder builder,
        Action<AzureQueueStorageHealthCheckOptions>? configureOptions = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREQUEUE_NAME,
           sp =>
           {
               var options = new AzureQueueStorageHealthCheckOptions();
               configureOptions?.Invoke(options);
               return new AzureQueueStorageHealthCheck(sp.GetRequiredService<QueueServiceClient>(), options);
           },
           failureStatus,
           tags,
           timeout));
    }

    /// <summary>
    /// Add a health check for Azure Queue Storage.
    /// </summary>
    /// <remarks>
    /// A <see cref="QueueServiceClient"/> service must be registered in the service container.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="configureOptions">Delegate for configuring the health check. Optional.</param>
    /// <param name="name">The health check name. Optional. If <see langword="null"/> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <see langword="null"/> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureQueueStorage(
        this IHealthChecksBuilder builder,
        Action<IServiceProvider, AzureQueueStorageHealthCheckOptions>? configureOptions = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? AZUREQUEUE_NAME,
           sp =>
           {
               var options = new AzureQueueStorageHealthCheckOptions();
               configureOptions?.Invoke(sp, options);
               return new AzureQueueStorageHealthCheck(sp.GetRequiredService<QueueServiceClient>(), options);
           },
           failureStatus,
           tags,
           timeout));
    }
}
