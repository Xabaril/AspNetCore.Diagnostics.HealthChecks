using Azure.Identity;
using Azure.Storage.Files.Shares;

namespace HealthChecks.Azure.Storage.Files.Shares.Tests;

public class FileConformanceTests : ConformanceTests<ShareServiceClient, AzureFileShareHealthCheck, AzureFileShareHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, ShareServiceClient>? clientFactory = null, Func<IServiceProvider, AzureFileShareHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureFileShare(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);

    protected override ShareServiceClient CreateClientForNonExistingEndpoint()
    {
        ShareClientOptions clientOptions = new();
        clientOptions.Retry.MaxRetries = 0; // don't enable retries (test runs few times faster)
        return new(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential(), clientOptions);
    }

    protected override AzureFileShareHealthCheck CreateHealthCheck(ShareServiceClient client, AzureFileShareHealthCheckOptions? options)
        => new(client, options);

    protected override AzureFileShareHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
