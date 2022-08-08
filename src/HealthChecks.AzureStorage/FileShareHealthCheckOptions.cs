namespace HealthChecks.AzureStorage;

/// <summary>
/// Represents a collection of settings that configure Azure Storage File Share Service health checks.
/// </summary>
public sealed class FileShareHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the name of the Azure Storage File Share whose health should be checked.
    /// </summary>
    /// <remarks>
    /// If the value is <see langword="null"/>, then no health check is performed for a specific share.
    /// </remarks>
    /// <value>An optional Azure Storage File Share name.</value>
    public string? ShareName { get; set; }
}
