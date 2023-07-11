using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusQueueHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusQueueHealthCheckOptions>, IHealthCheck
{
    private string? _connectionKey;

    protected override string ConnectionKey => _connectionKey ??= Prefix;

    private string queueKey => $"{ConnectionKey}_{Options.QueueName}";

    public AzureServiceBusQueueHealthCheck(AzureServiceBusQueueHealthCheckOptions options, ServiceBusClientProvider clientProvider)
        : base(options, clientProvider)
    {
        Guard.ThrowIfNull(options.QueueName, true);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (Options.UsePeekMode)
                await CheckWithReceiver().ConfigureAwait(false);
            else
                await CheckWithManagement().ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }

        async Task CheckWithReceiver()
        {
            var client = await ClientCache.GetOrAddAsyncDisposableAsync(Prefix, _ => CreateClient()).ConfigureAwait(false);
            var receiver = await ClientCache.GetOrAddAsyncDisposableAsync(
                $"{nameof(AzureServiceBusQueueHealthCheck)}_{queueKey}",
                _ => client.CreateReceiver(Options.QueueName))
                .ConfigureAwait(false);

            await receiver.PeekMessageAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        Task CheckWithManagement()
        {
            var managementClient = ClientCache.GetOrAdd(ConnectionKey, _ => CreateManagementClient());

            return managementClient.GetQueueRuntimePropertiesAsync(Options.QueueName, cancellationToken);
        }
    }
}
