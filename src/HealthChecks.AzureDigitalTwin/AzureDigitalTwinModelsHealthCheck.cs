using Azure.Core;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureDigitalTwin
{
    public class AzureDigitalTwinModelsHealthCheck
           : AzureDigitalTwinHealthCheck, IHealthCheck
    {
        private readonly string _hostName;
        private readonly string[] _models;

        public AzureDigitalTwinModelsHealthCheck(string clientId, string clientSecret, string tenantId, string hostName, string[] models)
            : base(clientId, clientSecret, tenantId)
        {
            _hostName = (!string.IsNullOrEmpty(hostName)) ? hostName : throw new ArgumentNullException(nameof(hostName));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public AzureDigitalTwinModelsHealthCheck(TokenCredential tokenCredential, string hostName, string[] models)
            : base(tokenCredential)
        {
            _hostName = (!string.IsNullOrEmpty(hostName)) ? hostName : throw new ArgumentNullException(nameof(hostName));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var digitalTwinClient = DigitalTwinClientConnections.GetOrAdd(ClientConnectionKey, _ => CreateDigitalTwinClient(_hostName));
                var response = digitalTwinClient.GetModelsAsync(cancellationToken: cancellationToken);
                var models = new List<DigitalTwinsModelData>();
                await foreach (var model in response)
                    models.Add(model);

                var unregistered = _models.Where(x => !models.Any(m => m.Id == x));
                return unregistered.Any()
                    ? new HealthCheckResult(
                        context.Registration.FailureStatus,
                        "The digital twin is out of sync with the models provided",
                        null,
                        new Dictionary<string, object>()
                        {
                            [nameof(unregistered)] = unregistered
                        })
                    : HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
