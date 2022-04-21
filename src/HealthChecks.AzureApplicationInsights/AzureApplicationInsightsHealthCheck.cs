using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.AzureApplicationInsights
{
    public class AzureApplicationInsightsHealthCheck : IHealthCheck
    {
        //from https://docs.microsoft.com/en-us/azure/azure-monitor/app/ip-addresses#outgoing-ports
        private readonly string[] m_appInsightsUrls =
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
                bool resourceExists = await ApplicationInsightsResourceExists(cancellationToken);
                if(resourceExists)
                {
                    return HealthCheckResult.Healthy();
                }
                else
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Could not find application insights resource. Searched resources: {string.Join(", ", m_appInsightsUrls)}");
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        public async Task<bool> ApplicationInsightsResourceExists(CancellationToken cancellationToken)
        {
            string path = $"/api/profiles/{_instrumentationKey}/appId";
            int index = 0;

            using (var httpClient = _httpClientFactory.CreateClient(AzureApplicationInsightsHealthCheckBuilderExtensions.AZUREAPPLICATIONINSIGHTS_NAME))
            {
                while (index < m_appInsightsUrls.Length)
                {
                    try
                    {
                        var uri = new Uri(m_appInsightsUrls[index++] + path);
                        HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionDispatchInfo.Capture(e).Throw();
                    }
                }
            }

            return false;
        }
    }
}
