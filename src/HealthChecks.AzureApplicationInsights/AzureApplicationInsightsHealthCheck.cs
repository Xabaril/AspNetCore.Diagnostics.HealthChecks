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
                return await CheckHealthAsyncInternal(context, cancellationToken);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }

            async Task<HealthCheckResult> CheckHealthAsyncInternal(HealthCheckContext context, CancellationToken cancellationToken = default)
            {
                string path = $"/api/profiles/{_instrumentationKey}/appId";
                ExceptionDispatchInfo? ex = null;
                int index = 0;

                while (index < m_appInsightsUrls.Length)
                {
                    using (var httpClient = _httpClientFactory.CreateClient(AzureApplicationInsightsHealthCheckBuilderExtensions.AZUREAPPLICATIONINSIGHTS_NAME))
                    {
                        try
                        {
                            var uri = new Uri(m_appInsightsUrls[index++] + path);
                            HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                            if (response.IsSuccessStatusCode)
                            {
                                return HealthCheckResult.Healthy();
                            }
                        }
                        catch (Exception e)
                        {
                            ex = ExceptionDispatchInfo.Capture(e);
                        }
                    }
                }

                if (ex == null)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: "Could not find application insights resource");
                }

                ex.Throw();
                // Unreachable code. It just to remove the vsCode error 'CheckHealthAsyncInternal(HealthCheckContext, CancellationToken)': not all code paths return a value
                return HealthCheckResult.Unhealthy();
            }
        }
    }
}
