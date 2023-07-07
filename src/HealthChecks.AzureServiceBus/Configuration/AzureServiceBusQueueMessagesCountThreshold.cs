using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Threshold configuration options for <see cref="AzureServiceBusQueueMessageCountThresholdHealthCheck"/>.
/// </summary>
public struct AzureServiceBusQueueMessagesCountThreshold
{
    /// <summary>
    /// Number of active/dead letter Service Bus messages in the queue before message health check returned <see cref="HealthStatus.Degraded"/>.
    /// </summary>
    public int DegradedThreshold { get; set; } = 5;

    /// <summary>
    /// Number of active/dead letter Service Bus messages in the queue before message health check returned <see cref="HealthStatus.Unhealthy"/>.
    /// </summary>
    public int UnhealthyThreshold { get; set; } = 10;

    public AzureServiceBusQueueMessagesCountThreshold()
    {
    }
}
