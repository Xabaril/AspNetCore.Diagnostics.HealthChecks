using Azure.Core;
using Azure.Messaging.EventHubs;

namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Configuration options for <see cref="AzureEventHubHealthCheck"/>.
/// </summary>
public class AzureEventHubHealthCheckOptions
{
    /// <summary>
    /// The azure event hub connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The azure event hub name.
    /// </summary>
    public string? EventHubName { get; set; }

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

    /// <summary>
    /// The event hub connection to use for authenticating and connecting.
    /// </summary>
    public EventHubConnection? Connection { get; set; }
}
