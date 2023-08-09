using Azure.Messaging.EventHubs.Producer;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureEventHubHealthCheck : IHealthCheck
{
    private const string ENTITY_PATH_SEGMENT = "EntityPath=";
    private readonly AzureEventHubHealthCheckOptions _options;

    private string? _connectionKey;

    private string ConnectionKey => _connectionKey ??= _options.ConnectionString is null
        ? $"{_options.FullyQualifiedNamespace}_{_options.EventHubName}"
        : GetFullConnectionString();

    public AzureEventHubHealthCheck(AzureEventHubHealthCheckOptions options)
    {
        _options = options;

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            Guard.ThrowIfNull(options.EventHubName, true);
            return;
        }

        if (options.Credential is not null)
        {
            Guard.ThrowIfNull(options.FullyQualifiedNamespace, true);
            Guard.ThrowIfNull(options.EventHubName, true);
            return;
        }

        if (options.Connection is not null)
        {
            _connectionKey = $"{options.Connection.FullyQualifiedNamespace}_{options.Connection.EventHubName}";
            return;
        }

        throw new ArgumentException("A connection string, TokenCredential or EventHubConnection must be set!",
            nameof(options));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await ClientCache.GetOrAddAsyncDisposableAsync(ConnectionKey, _ => CreateClient(context)).ConfigureAwait(false);

            _ = await client.GetEventHubPropertiesAsync(cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private EventHubProducerClient CreateClient(HealthCheckContext context)
    {
        var clientOptions = CreateClientOptions(context);

        if (_options.ConnectionString is not null)
            return new EventHubProducerClient(GetFullConnectionString(), clientOptions);

        if (_options.Connection is not null)
            return new EventHubProducerClient(_options.Connection, clientOptions);

        return new EventHubProducerClient(_options.FullyQualifiedNamespace, _options.EventHubName, _options.Credential, clientOptions);
    }

    private string GetFullConnectionString()
    {
        string connectionString = _options.ConnectionString!;

        if (!connectionString.Contains(ENTITY_PATH_SEGMENT))
            connectionString = $"{connectionString};{ENTITY_PATH_SEGMENT}{_options.EventHubName}";
        return connectionString;
    }

    private static EventHubProducerClientOptions CreateClientOptions(HealthCheckContext context) => new()
    {
        ConnectionOptions = new()
        {
            ConnectionIdleTimeout = context.Registration.Timeout,
        }
    };
}
