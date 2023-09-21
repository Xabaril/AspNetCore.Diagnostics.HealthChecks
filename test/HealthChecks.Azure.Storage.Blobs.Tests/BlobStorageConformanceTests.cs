using Azure.Identity;
using Azure.Storage.Blobs;

namespace HealthChecks.Azure.Storage.Blobs.Tests;

public class BlobStorageConformanceTests : ConformanceTests<BlobServiceClient, AzureBlobStorageHealthCheck, AzureBlobStorageHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, BlobServiceClient>? clientFactory = null, Func<IServiceProvider, AzureBlobStorageHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureBlobStorage(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);

    protected override BlobServiceClient CreateClientForNonExistingEndpoint()
    {
        BlobClientOptions clientOptions = new();
        clientOptions.Retry.MaxRetries = 0; // don't enable retries (test runs few times faster)
        return new(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential(), clientOptions);
    }

    protected override AzureBlobStorageHealthCheck CreateHealthCheck(BlobServiceClient client, AzureBlobStorageHealthCheckOptions? options)
        => new(client, options);

    protected override AzureBlobStorageHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
