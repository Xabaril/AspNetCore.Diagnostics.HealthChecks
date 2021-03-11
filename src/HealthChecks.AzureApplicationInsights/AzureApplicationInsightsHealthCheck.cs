using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

        public AzureApplicationInsightsHealthCheck(string instrumentationKey)
        {
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                throw new ArgumentNullException(nameof(instrumentationKey));
            }

            _instrumentationKey = instrumentationKey;         
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
        }
        
        private async Task<HealthCheckResult> CheckHealthAsyncInternal(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            string path = $"/api/profiles/{_instrumentationKey}/appId";
            Exception ex = null;
            int index = 0;

            while (index < m_appInsightsUrls.Length)
            {
                using(var httpClient = new HttpClient())
                {
                    try
                    {
                        var uri = new Uri(m_appInsightsUrls[index++] + path);
                        HttpResponseMessage response = await httpClient.GetAsync(uri,cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            return HealthCheckResult.Healthy();
                        }
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                }
            }

            if(ex == null)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, description: "Could not find application insights resource");
            }

            throw ex;
        }
    }
}
