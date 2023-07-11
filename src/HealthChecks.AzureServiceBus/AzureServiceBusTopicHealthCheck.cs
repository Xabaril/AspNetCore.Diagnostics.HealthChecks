using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusTopicHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusTopicHealthCheckOptions>, IHealthCheck
{
    private string? _connectionKey;

    protected override string ConnectionKey => _connectionKey ??= Prefix;

    public AzureServiceBusTopicHealthCheck(AzureServiceBusTopicHealthCheckOptions options, ServiceBusClientProvider clientProvider)
        : base(options, clientProvider)
    {
        Guard.ThrowIfNull(options.TopicName, true);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var managementClient = ClientCache.GetOrAdd(ConnectionKey, _ => CreateManagementClient());

            _ = await managementClient.GetTopicRuntimePropertiesAsync(Options.TopicName, cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
