using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace HealthChecks.CosmosDb.Tests;

public class CosmosDbConformanceTests : ConformanceTests<CosmosClient, AzureCosmosDbHealthCheck, AzureCosmosDbHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, CosmosClient>? clientFactory = null, Func<IServiceProvider, AzureCosmosDbHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureCosmosDB(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);

    protected override CosmosClient CreateClientForNonExistingEndpoint()
        => new CosmosClient(
            "https://www.thisisnotarealurl.com",
            new DefaultAzureCredential(),
            new CosmosClientOptions()
            {
                RequestTimeout = TimeSpan.FromMilliseconds(10),
                MaxRetryAttemptsOnRateLimitedRequests = 0,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.Zero,
                ApplicationRegion = Regions.EastUS2,
            });

    protected override AzureCosmosDbHealthCheck CreateHealthCheck(CosmosClient client, AzureCosmosDbHealthCheckOptions? options)
        => new(client, options);

    protected override AzureCosmosDbHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
