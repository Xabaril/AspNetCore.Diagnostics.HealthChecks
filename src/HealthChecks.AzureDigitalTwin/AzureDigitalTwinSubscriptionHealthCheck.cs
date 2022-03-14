using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Rest;

namespace HealthChecks.AzureDigitalTwin
{
    public class AzureDigitalTwinSubscriptionHealthCheck
           : AzureDigitalTwinHealthCheck, IHealthCheck
    {
        public AzureDigitalTwinSubscriptionHealthCheck(string clientId, string clientSecret, string tenantId)
            : base(clientId, clientSecret, tenantId)
        { }

        public AzureDigitalTwinSubscriptionHealthCheck(ServiceClientCredentials serviceClientCredentials)
            : base(serviceClientCredentials)
        { }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var managementClient = ManagementClientConnections.GetOrAdd(ClientConnectionKey, _ => CreateManagementClient());
                _ = await managementClient.Operations.ListWithHttpMessagesAsync(cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
