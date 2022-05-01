using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.AzureApplicationInsights
{
    public class AzureApplicationInsightsHealthCheck : IHealthCheck
    {
        //from https://docs.microsoft.com/en-us/azure/azure-monitor/app/ip-addresses#outgoing-ports
        private readonly string[] _appInsightsUrls =
        {
            "https://dc.applicationinsights.azure.com",
            "https://dc.applicationinsights.microsoft.com",
            "https://dc.services.visualstudio.com"
        };
        private readonly string _instrumentationKey;

        private readonly IHttpClientFactory _httpClientFactory;

        public AzureApplicationInsightsHealthCheck(string instrumentationKey, IHttpClientFactory httpClientFactory)
        {
            _instrumentationKey = instrumentationKey ?? throw new ArgumentNullException(nameof(instrumentationKey));       
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory)); 
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                bool resourceExists = await ApplicationInsightsResourceExistsAsync(cancellationToken);
                if (resourceExists)
                {
                    return HealthCheckResult.Healthy();
                }
                else
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Could not find application insights resource. Searched resources: {string.Join(", ", _appInsightsUrls)}");
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private async Task<bool> ApplicationInsightsResourceExistsAsync(CancellationToken cancellationToken)
        {
            using (var httpClient = _httpClientFactory.CreateClient(AzureApplicationInsightsHealthCheckBuilderExtensions.AZUREAPPLICATIONINSIGHTS_NAME))
            {
                string path = $"/api/profiles/{_instrumentationKey}/appId";
                int index = 0;

                while (index < _appInsightsUrls.Length)
                {
                    try
                    {
                        var uri = new Uri(_appInsightsUrls[index++] + path);
                        HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        // We reached the end of the url list and there's no more url to check
                        if (index == _appInsightsUrls.Length)
                        {
                            ExceptionDispatchInfo.Capture(e).Throw();
                        }
                    }
                }
            }

            return false;
        }
    }
}
