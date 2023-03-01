using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusQueueHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusQueueHealthCheckOptions>, IHealthCheck
{
    private string? _connectionKey;

    protected override string ConnectionKey => _connectionKey ??= $"{Prefix}_{Options.QueueName}";

    public AzureServiceBusQueueHealthCheck(AzureServiceBusQueueHealthCheckOptions options)
        : base(options)
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
            return Options.IsExceptionDetailsRequired
                ? new HealthCheckResult(context.Registration.FailureStatus, exception: ex)
                : new HealthCheckResult(context.Registration.FailureStatus);

        }

        Task CheckWithReceiver()
        {
            var client = ClientConnections.GetOrAdd(ConnectionKey, _ => CreateClient());
            var receiver = ServiceBusReceivers.GetOrAdd(
                $"{nameof(AzureServiceBusQueueHealthCheck)}_{ConnectionKey}",
                client.CreateReceiver(Options.QueueName));

            return receiver.PeekMessageAsync(cancellationToken: cancellationToken);
        }

        Task CheckWithManagement()
        {
            var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, _ => CreateManagementClient());

            return managementClient.GetQueueRuntimePropertiesAsync(Options.QueueName, cancellationToken);
        }
    }
}
