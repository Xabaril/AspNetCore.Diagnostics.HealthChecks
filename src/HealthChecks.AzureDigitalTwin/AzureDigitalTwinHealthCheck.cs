using Microsoft.Azure.Management.DigitalTwins;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Rest;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureDigitalTwin
{
    public class AzureDigitalTwinHealthCheck
           : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, AzureDigitalTwinsManagementClient> _managementClientConnections
            = new ConcurrentDictionary<string, AzureDigitalTwinsManagementClient>();

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly ServiceClientCredentials _credentials;

        public AzureDigitalTwinHealthCheck(string clientId, string clientSecret, string tenantId)
        {
            _clientId = (!string.IsNullOrEmpty(clientId)) ? clientId : throw new ArgumentNullException(nameof(clientId));
            _clientSecret = (!string.IsNullOrEmpty(clientSecret)) ? clientSecret : throw new ArgumentNullException(nameof(clientSecret));
            _tenantId = (!string.IsNullOrEmpty(tenantId)) ? tenantId : throw new ArgumentNullException(nameof(tenantId));

            _credentials = new AzureCredentialsFactory().FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
        }

        public AzureDigitalTwinHealthCheck(ServiceClientCredentials credentials)
        {
            _credentials = (credentials is not null) ? credentials : throw new ArgumentNullException(nameof(credentials));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_managementClientConnections.TryGetValue(_clientId, out var managementClient))
                {
                    managementClient = new AzureDigitalTwinsManagementClient(new Uri("https://management.azure.com"), _credentials);

                    if (!_managementClientConnections.TryAdd(_clientId, managementClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "No digital twin administration client connection can't be added into dictionary.");
                    }
                }

                await managementClient.Operations.ListWithHttpMessagesAsync(cancellationToken: cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
