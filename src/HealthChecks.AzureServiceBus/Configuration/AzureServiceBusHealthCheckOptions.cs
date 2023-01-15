using Azure.Core;

namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Configuration options for <see cref="AzureServiceBusHealthCheck"/>.
/// </summary>
public class AzureServiceBusHealthCheckOptions
{
    /// <summary>
    /// The azure event hub connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The azure event hub fully qualified namespace.
    /// </summary>
    /// <remarks>
    /// Must be used in conjunction with the <see cref="Credential"/> property.
    /// </remarks>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// The token credential for authentication.
    /// </summary>
    /// <remarks>
    /// Must be used in conjunction with the <see cref="FullyQualifiedNamespace"/> property.
    /// </remarks>
    public TokenCredential? Credential { get; set; }
}
