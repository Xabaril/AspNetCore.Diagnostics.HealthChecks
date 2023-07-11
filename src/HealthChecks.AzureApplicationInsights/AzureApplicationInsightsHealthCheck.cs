using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureApplicationInsights
{
    public class AzureApplicationInsightsHealthCheck : IHealthCheck
    {
        // from https://docs.microsoft.com/en-us/azure/azure-monitor/app/ip-addresses#outgoing-ports
        private readonly string[] _appInsightsUrls =
        {
            "https://dc.applicationinsights.azure.com",
            "https://dc.applicationinsights.microsoft.com",
            "https://dc.services.visualstudio.com"
        };
        private readonly string _instrumentationKey;

        private readonly IHttpClientFactory _httpClientFactory;

        /// <inheritdoc />
        public AzureApplicationInsightsHealthCheck(string instrumentationKey, IHttpClientFactory httpClientFactory)
        {
            _instrumentationKey = Guard.ThrowIfNull(instrumentationKey, throwOnEmptyString: true);
            _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                bool resourceExists = await ApplicationInsightsResourceExistsAsync(cancellationToken).ConfigureAwait(false);
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
            using var httpClient = _httpClientFactory.CreateClient(AzureApplicationInsightsHealthCheckBuilderExtensions.AZUREAPPLICATIONINSIGHTS_NAME);

            string path = $"/api/profiles/{_instrumentationKey}/appId";
            int index = 0;
            var exceptions = new List<Exception>();
            while (index < _appInsightsUrls.Length)
            {
                try
                {
                    var uri = new Uri(_appInsightsUrls[index++] + path);
                    HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            // All endpoints threw exceptions
            if (exceptions.Count == _appInsightsUrls.Length)
            {
                throw new AggregateException(exceptions.ToArray());
            }

            // No success responses were returned and at least one endpoint returned an unsuccesful response
            return false;
        }
    }
}
