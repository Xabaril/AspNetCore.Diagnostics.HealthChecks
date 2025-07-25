using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;

namespace HealthChecks.AzureServiceBus;

public abstract class AzureServiceBusHealthCheck<TOptions> where TOptions : AzureServiceBusHealthCheckOptions
{
    protected TOptions Options { get; }

    private readonly ServiceBusClientProvider _clientProvider;

    [Obsolete("Please, override ConnectionKey property instead.")]
    protected string Prefix => Options.ConnectionString ?? Options.FullyQualifiedNamespace!;

    protected virtual string ConnectionKey => Options.ConnectionString ?? Options.FullyQualifiedNamespace!;

    protected AzureServiceBusHealthCheck(TOptions options, ServiceBusClientProvider clientProvider)
    {
        Options = options;
        _clientProvider = clientProvider;

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            return;

        if (!string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace))
        {
            options.Credential ??= new DefaultAzureCredential();
            return;
        }

        throw new ArgumentException("A connection string or endpoint must be set!", nameof(options));
    }

    protected ServiceBusClient CreateClient() =>
        !string.IsNullOrWhiteSpace(Options.ConnectionString)
            ? _clientProvider.CreateClient(Options.ConnectionString)
            : _clientProvider.CreateClient(Options.FullyQualifiedNamespace, Options.Credential!);

    protected ServiceBusAdministrationClient CreateManagementClient() =>
        !string.IsNullOrWhiteSpace(Options.ConnectionString)
            ? _clientProvider.CreateManagementClient(Options.ConnectionString)
            : _clientProvider.CreateManagementClient(Options.FullyQualifiedNamespace, Options.Credential!);
}
