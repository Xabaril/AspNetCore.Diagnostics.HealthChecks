using Azure.Core;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureDigitalTwin;

public class AzureDigitalTwinInstanceHealthCheck
       : AzureDigitalTwinHealthCheck, IHealthCheck
{
    private readonly string _hostName;
    private readonly string _instanceName;

    public AzureDigitalTwinInstanceHealthCheck(string clientId, string clientSecret, string tenantId, string hostName, string instanceName)
        : base(clientId, clientSecret, tenantId)
    {
        _hostName = Guard.ThrowIfNull(hostName, true);
        _instanceName = Guard.ThrowIfNull(instanceName, true);
    }

    public AzureDigitalTwinInstanceHealthCheck(TokenCredential tokenCredential, string hostName, string instanceName)
        : base(tokenCredential)
    {
        _hostName = Guard.ThrowIfNull(hostName, true);
        _instanceName = Guard.ThrowIfNull(instanceName, true);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var digitalTwinClient = DigitalTwinClientConnections.GetOrAdd(ClientConnectionKey, _ => CreateDigitalTwinClient(_hostName));
            _ = await digitalTwinClient.GetDigitalTwinAsync<BasicDigitalTwin>(_instanceName, cancellationToken: cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();

        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
