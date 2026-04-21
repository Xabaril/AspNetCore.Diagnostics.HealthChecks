using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusTopicHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusTopicHealthCheckOptions>, IHealthCheck
{
    private readonly string _topicKey;

    public AzureServiceBusTopicHealthCheck(AzureServiceBusTopicHealthCheckOptions options, ServiceBusClientProvider clientProvider)
        : base(options, clientProvider)
    {
        Guard.ThrowIfNull(options.TopicName, true);

        _topicKey = $"{nameof(AzureServiceBusTopicHealthCheck)}_{ConnectionKey}_{Options.TopicName}";
    }

    public AzureServiceBusTopicHealthCheck(AzureServiceBusTopicHealthCheckOptions options)
        : this(options, new ServiceBusClientProvider())
    { }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (Options.UseCreateMessageBatchAsyncMode)
                await CheckWithSender().ConfigureAwait(false);
            else
                await CheckWithManagement().ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }

        async Task CheckWithSender()
        {
            var client = await ClientCache.GetOrAddAsyncDisposableAsync(ConnectionKey, _ => CreateClient()).ConfigureAwait(false);
            var sender = await ClientCache.GetOrAddAsyncDisposableAsync(
                _topicKey,
                _ => client.CreateSender(Options.TopicName))
                .ConfigureAwait(false);

            await sender.CreateMessageBatchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        Task CheckWithManagement()
        {
            var managementClient = ClientCache.GetOrAdd(ConnectionKey, _ => CreateManagementClient());

            return managementClient.GetTopicRuntimePropertiesAsync(Options.TopicName, cancellationToken);
        }
    }
}
