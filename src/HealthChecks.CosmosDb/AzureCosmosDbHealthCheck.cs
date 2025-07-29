using System.Collections.ObjectModel;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CosmosDb;

/// <summary>
/// Azure Cosmos DB health check.
/// </summary>
public sealed class AzureCosmosDbHealthCheck : IHealthCheck
{
    private readonly CosmosClient _cosmosClient;
    private readonly AzureCosmosDbHealthCheckOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.name", nameof(AzureCosmosDbHealthCheck) },
                    { "health_check.task", "ready" },
                    { "db.system.name", "azurecosmos" }
    };

    /// <summary>
    /// Creates new instance of Azure Cosmos DB health check.
    /// </summary>
    /// <param name="cosmosClient">
    /// The <see cref="CosmosClient"/> used to perform Azure Cosmos DB operations.
    /// Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/>,
    /// so this should be the exact same instance used by other parts of the application.
    /// </param>
    /// <param name="options">Optional settings used by the health check.</param>
    public AzureCosmosDbHealthCheck(CosmosClient cosmosClient, AzureCosmosDbHealthCheckOptions? options)
    {
        _cosmosClient = Guard.ThrowIfNull(cosmosClient);
        _options = options ?? new();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            checkDetails.Add("server.address", _cosmosClient.Endpoint.Host);
            checkDetails.Add("server.port", _cosmosClient.Endpoint.Port);
            await _cosmosClient.ReadAccountAsync().ConfigureAwait(false);

            if (_options.DatabaseId != null)
            {
                checkDetails.Add("db.namespace", _options.DatabaseId);
                var database = _cosmosClient.GetDatabase(_options.DatabaseId);
                await database.ReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (_options.ContainerIds != null)
                {
                    foreach (var container in _options.ContainerIds)
                    {
                        await database
                            .GetContainer(container)
                            .ReadContainerAsync(cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }

            return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
