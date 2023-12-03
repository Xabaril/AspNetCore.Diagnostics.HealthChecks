using Azure.Messaging.EventHubs.Producer;
using HealthChecks.Azure.Messaging.EventHubs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureEventHubHealthCheck"/>.
/// </summary>
public static class AzureEventHubHealthChecksBuilderExtensions
{
    private const string HEALTH_CHECK_NAME = "azure_event_hub";

    /// <summary>
    /// Add a health check for Azure Event Hub by registering <see cref="AzureEventHubHealthCheck"/> for given <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add <see cref="HealthCheckRegistration"/> to.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="EventHubProducerClient" /> instance.
    /// When not provided, <see cref="EventHubProducerClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="healthCheckName">The health check name. Optional. If <c>null</c> the name 'azure_event_hub' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureEventHub(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, EventHubProducerClient>? clientFactory = default,
        string? healthCheckName = HEALTH_CHECK_NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           string.IsNullOrEmpty(healthCheckName) ? HEALTH_CHECK_NAME : healthCheckName!,
           sp => new AzureEventHubHealthCheck(clientFactory?.Invoke(sp) ?? sp.GetRequiredService<EventHubProducerClient>()),
           failureStatus,
           tags,
           timeout));
    }
}
