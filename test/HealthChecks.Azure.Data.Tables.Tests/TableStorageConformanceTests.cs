using Azure.Data.Tables;
using Azure.Identity;

namespace HealthChecks.Azure.Data.Tables.Tests;

public class TablesConformanceTests : ConformanceTests<TableServiceClient, AzureTableServiceHealthCheck, AzureTableServiceHealthCheckOptions>
{
    protected override IHealthChecksBuilder AddHealthCheck(IHealthChecksBuilder builder, Func<IServiceProvider, TableServiceClient>? clientFactory = null, Func<IServiceProvider, AzureTableServiceHealthCheckOptions>? optionsFactory = null, string? healthCheckName = null, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        => builder.AddAzureTable(clientFactory, optionsFactory, healthCheckName, failureStatus, tags, timeout);

    protected override TableServiceClient CreateClientForNonExistingEndpoint()
    {
        TableClientOptions clientOptions = new();
        clientOptions.Retry.MaxRetries = 0; // don't enable retries (test runs few times faster)
        return new(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential(), clientOptions);
    }

    protected override AzureTableServiceHealthCheck CreateHealthCheck(TableServiceClient client, AzureTableServiceHealthCheckOptions? options)
        => new(client, options);

    protected override AzureTableServiceHealthCheckOptions CreateHealthCheckOptions()
        => new();
}
