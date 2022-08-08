namespace HealthChecks.AzureStorage;

/// <summary>
/// Represents a collection of settings that configure Azure Queue Storage health checks.
/// </summary>
public sealed class QueueStorageHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the name of the Azure Queue whose health should be checked.
    /// </summary>
    /// <remarks>
    /// If the value is <c>null</c>, then no health check is performed for a specific queue.
    /// </remarks>
    /// <value>An optional Azure Queue name.</value>
    public string? QueueName { get; set; }
}
