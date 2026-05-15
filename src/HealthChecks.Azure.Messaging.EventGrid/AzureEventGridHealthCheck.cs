namespace HealthChecks.Azure.Messaging.EventGrid;

public sealed class AzureEventGridHealthCheck : IHealthCheck
{
    private readonly EventGridPublisherClient _client;

    public AzureEventGridHealthCheck(EventGridPublisherClient client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Send a ping event to verify connectivity
            var healthCheckEvent = new EventGridEvent(
                "HealthCheck",
                "HealthCheck.Ping",
                "1.0",
                new { Timestamp = DateTimeOffset.UtcNow });

            await _client.SendEventAsync(healthCheckEvent, cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}