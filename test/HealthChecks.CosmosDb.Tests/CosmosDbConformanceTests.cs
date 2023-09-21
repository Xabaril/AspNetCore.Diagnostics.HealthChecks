using Microsoft.Azure.Cosmos;

namespace HealthChecks.CosmosDb.Tests;

public class CosmosDbConformanceTests : ConformanceTests<CosmosClient, AzureCosmosDbHealthCheck, AzureCosmosDbHealthCheckOptions>
{
    // Sample raw connection string taken from https://github.com/Azure/azure-cosmos-dotnet-v3/blob/258d960ae3caa3ad989f60d5e656544c35006d0a/Microsoft.Azure.Cosmos/tests/Microsoft.Azure.Cosmos.Tests/CosmosClientTests.cs#L34
    private const string ConnectionString = "AccountEndpoint=https://localtestcosmos.documents.azure.com:443/;AccountKey=425Mcv8CXQqzRNCgFNjIhT424GK99CKJvASowTnq15Vt8LeahXTcN5wt3342vQ==;";

    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, CosmosClient>? clientFactory = null, Func<IServiceProvider, AzureCosmosDbHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureCosmosDB(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);

    protected override CosmosClient CreateClientForNonExistingEndpoint()
        => new(ConnectionString);

    protected override AzureCosmosDbHealthCheck CreateHealthCheck(CosmosClient client, AzureCosmosDbHealthCheckOptions? options)
        => new(client, options);

    protected override AzureCosmosDbHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
