using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusSubscriptionHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>, IHealthCheck
{
    private string? _connectionKey;

    protected override string ConnectionKey =>
        _connectionKey ??= $"{Prefix}_{Options.TopicName}_{Options.SubscriptionName}";

    public AzureServiceBusSubscriptionHealthCheck(AzureServiceBusSubscriptionHealthCheckHealthCheckOptions options)
        : base(options)
    {
        Guard.ThrowIfNull(options.TopicName, true);
        Guard.ThrowIfNull(options.SubscriptionName, true);
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
            var client = await ClientCache.GetOrAddAsyncDisposableAsync(ConnectionKey, _ => CreateClient()).ConfigureAwait(false);
            var receiver = await ClientCache.GetOrAddAsyncDisposableAsync(
                $"{nameof(AzureServiceBusSubscriptionHealthCheck)}_{ConnectionKey}",
                _ => client.CreateReceiver(Options.TopicName, Options.SubscriptionName))
                .ConfigureAwait(false);

            await receiver.PeekMessageAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        Task CheckWithManagement()
        {
            var managementClient = ClientCache.GetOrAdd(ConnectionKey, _ => CreateManagementClient());

            return managementClient.GetSubscriptionRuntimePropertiesAsync(
                Options.TopicName, Options.SubscriptionName, cancellationToken);
        }
    }
}
