using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace HealthChecks.IdentityServer
{
    public class IdentityServerHealthCheck
        : IHealthCheck
    {
        const string IDENTITY_SERVER_DISCOVER_CONFIGURATION_SEGMENT = ".well-known/openid-configuration"; 

        private readonly Func<HttpClient> _httpClientFactory;
        public IdentityServerHealthCheck(Func<HttpClient> httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory();
                var response = await httpClient.GetAsync(IDENTITY_SERVER_DISCOVER_CONFIGURATION_SEGMENT, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content { await response.Content.ReadAsStringAsync() }");
                }

                // is it a valid json document?
                JsonReaderWriterFactory.CreateJsonReader(await response.Content.ReadAsStreamAsync(), new XmlDictionaryReaderQuotas()).Read();

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
