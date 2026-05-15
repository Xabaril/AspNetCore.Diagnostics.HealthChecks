using Azure.Core;

namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Base class for configuration options for descendants of the <see cref="AzureServiceBusHealthCheck{TOptions}"/> class.
/// </summary>
public abstract class AzureServiceBusHealthCheckOptions
{
    /// <summary>
    /// The azure service bus connection string.
    /// </summary>
    /// <remarks>
    /// If <see cref="ConnectionString"/> is set, it overrides <see cref="FullyQualifiedNamespace"/> and <see cref="Credential"/>.
    /// </remarks>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The azure service bus fully qualified namespace.
    /// </summary>
    /// <remarks>
    /// Used in conjunction with the <see cref="Credential"/> property.
    /// </remarks>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// The token credential for authentication.
    /// </summary>
    /// <remarks>
    /// If <see cref="Credential"/> is not set, it defaults to <see cref="Azure.Identity.DefaultAzureCredential"/>.
    /// </remarks>
    public TokenCredential? Credential { get; set; }
}
