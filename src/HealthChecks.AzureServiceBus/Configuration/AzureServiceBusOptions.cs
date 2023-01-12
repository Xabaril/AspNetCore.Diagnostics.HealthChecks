using Azure.Core;

namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Azure Service Bus configuration options.
/// </summary>
public class AzureServiceBusOptions
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
    public string? Endpoint { get; set; }

    /// <summary>
    /// The token credential for authentication.
    /// </summary>
    /// <remarks>
    /// Must be used in conjunction with the <see cref="Endpoint"/> property.
    /// </remarks>
    public TokenCredential? Credential { get; set; }
}
