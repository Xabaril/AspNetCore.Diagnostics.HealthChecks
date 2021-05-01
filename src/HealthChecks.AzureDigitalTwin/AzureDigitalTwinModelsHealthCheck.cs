using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureDigitalTwin
{
    public class AzureDigitalTwinModelsHealthCheck
           : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, DigitalTwinsClient> _managementClientConnections
            = new ConcurrentDictionary<string, DigitalTwinsClient>();

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly string _hostName;
        private readonly string[] _models;

        public AzureDigitalTwinModelsHealthCheck(string clientId, string clientSecret, string tenantId, string hostName, string[] models)
        {
            _clientId = (!string.IsNullOrEmpty(clientId)) ? clientId : throw new ArgumentNullException(nameof(clientId));
            _clientSecret = (!string.IsNullOrEmpty(clientSecret)) ? clientSecret : throw new ArgumentNullException(nameof(clientSecret));
            _tenantId = (!string.IsNullOrEmpty(tenantId)) ? tenantId : throw new ArgumentNullException(nameof(tenantId));
            _hostName = (!string.IsNullOrEmpty(hostName)) ? hostName : throw new ArgumentNullException(nameof(hostName));
            _models = models.Any() ? models : throw new ArgumentNullException(nameof(models));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_managementClientConnections.TryGetValue(_clientId, out var managementClient))
                {
                    var credentials = new ClientSecretCredential(_tenantId, _clientId, _clientSecret);
                    managementClient = new DigitalTwinsClient(new Uri(_hostName), credentials);

                    if (!_managementClientConnections.TryAdd(_clientId, managementClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "No digital twin administration client connection can't be added into dictionary.");
                    }
                }

                var response = managementClient.GetModelsAsync(cancellationToken: cancellationToken);
                var models = new List<DigitalTwinsModelData>();
                await foreach (var model in response) models.Add(model);

                var unregistered = _models.Where(x => !models.Any(m => m.Id == x));
                var unmapped = models.Where(x => !_models.Contains(x.Id)).Select(x => x.Id);
                return unregistered.Any() || unmapped.Any()
                    ? new HealthCheckResult(
                        context.Registration.FailureStatus,
                        "The digital twin is out of sync with the models provided",
                        null,
                        new Dictionary<string, object>()
                        {
                            [nameof(unregistered)] = unregistered,
                            [nameof(unmapped)] = unmapped
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
