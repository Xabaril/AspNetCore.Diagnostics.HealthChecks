using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureEventHubHealthCheck : IHealthCheck
{
    private const string ENTITY_PATH_SEGMENT = "EntityPath=";

    private readonly string? _connectionString;
    private readonly string? _endpoint;
    private readonly string? _eventHubName;
    private readonly TokenCredential? _tokenCredential;
    private readonly EventHubConnection? _connection;

    private string? _connectionKey;

    private string ConnectionKey =>
        _connectionKey ??= _connectionString ?? $"{_endpoint}_{_eventHubName}";

    public AzureEventHubHealthCheck(string connectionString, string eventHubName)
    {
        Guard.ThrowIfNull(connectionString, true);
        Guard.ThrowIfNull(eventHubName, true);

        _connectionString = connectionString.Contains(ENTITY_PATH_SEGMENT) ? connectionString : $"{connectionString};{ENTITY_PATH_SEGMENT}{eventHubName}";
    }

    public AzureEventHubHealthCheck(EventHubConnection connection)
    {
        Guard.ThrowIfNull(connection);

        _connectionKey = $"{connection.FullyQualifiedNamespace}_{connection.EventHubName}";
        _connection = connection;
    }

    public AzureEventHubHealthCheck(string endpoint, string eventHubName, TokenCredential tokenCredential)
    {
        _endpoint = Guard.ThrowIfNull(endpoint, true);
        _eventHubName = Guard.ThrowIfNull(eventHubName, true);
        _tokenCredential = Guard.ThrowIfNull(tokenCredential);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await ClientCache.GetOrAddAsyncDisposableAsync(ConnectionKey, _ => CreateClient()).ConfigureAwait(false);

            _ = await client.GetEventHubPropertiesAsync(cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private EventHubProducerClient CreateClient()
    {
        if (_connectionString is not null)
            return new EventHubProducerClient(_connectionString);

        if (_connection is not null)
            return new EventHubProducerClient(_connection);

        return new EventHubProducerClient(_endpoint, _eventHubName, _tokenCredential);
    }
}
