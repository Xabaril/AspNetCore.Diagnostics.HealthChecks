using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using HealthChecks.AzureServiceBus.Configuration;
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

    private string ConnectionKey => _connectionKey ??= _connectionString ?? $"{_endpoint}_{_eventHubName}";

    public AzureEventHubHealthCheck(AzureEventHubOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            Guard.ThrowIfNull(options.EventHubName, true);

            string connectionString = options.ConnectionString!;
            _connectionString = connectionString.Contains(ENTITY_PATH_SEGMENT)
                ? connectionString
                : $"{connectionString};{ENTITY_PATH_SEGMENT}{options.EventHubName}";

            return;
        }

        if (options.Credential is not null)
        {
            _endpoint = Guard.ThrowIfNull(options.Endpoint, true);
            _eventHubName = Guard.ThrowIfNull(options.EventHubName, true);
            _tokenCredential = options.Credential;
            return;
        }

        if (options.Connection is not null)
        {
            _connection = options.Connection;
            _connectionKey = $"{_connection.FullyQualifiedNamespace}_{_connection.EventHubName}";
            return;
        }

        throw new ArgumentException("A connection string, TokenCredential or EventHubConnection must be set!",
            nameof(options));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await ClientCache.GetOrAddAsyncDisposableAsync(ConnectionKey, _ => CreateClient())
                .ConfigureAwait(false);

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
