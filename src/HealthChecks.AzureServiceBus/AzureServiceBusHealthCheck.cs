using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;

namespace HealthChecks.AzureServiceBus;

public abstract class AzureServiceBusHealthCheck<TOptions> where TOptions : AzureServiceBusHealthCheckOptions
{
    protected TOptions Options { get; }

    protected string Prefix => Options.ConnectionString ?? Options.FullyQualifiedNamespace!;

    protected abstract string ConnectionKey { get; }

    protected AzureServiceBusHealthCheck(TOptions options)
    {
        Options = options;

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            return;

        if (!string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace))
        {
            Guard.ThrowIfNull(options.Credential);
            return;
        }

        throw new ArgumentException("A connection string or endpoint must be set!", nameof(options));
    }

    protected ServiceBusClient CreateClient() =>
        Options.Credential is null
            ? new ServiceBusClient(Options.ConnectionString)
            : new ServiceBusClient(Options.FullyQualifiedNamespace, Options.Credential);

    protected ServiceBusAdministrationClient CreateManagementClient() =>
        Options.Credential is null
            ? new ServiceBusAdministrationClient(Options.ConnectionString)
            : new ServiceBusAdministrationClient(Options.FullyQualifiedNamespace, Options.Credential);
}
