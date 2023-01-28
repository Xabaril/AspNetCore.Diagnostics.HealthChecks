using Azure.Core;

namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Base class for configuration options for descendants of the <see cref="AzureServiceBusHealthCheck"/>.
/// </summary>
public abstract class AzureServiceBusHealthCheckOptions
{
    /// <summary>
    /// The azure service bus connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The azure service bus fully qualified namespace.
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
