using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Uris
{
    public class UriHealthCheck
        : IHealthCheck
    {
        private readonly UriHealthCheckOptions _options;
        public UriHealthCheck(UriHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var defaultHttpMethod = _options.HttpMethod;
            var defaultCodes = _options.ExpectedHttpCodes;
            var idx = 0;

            try
            {
                foreach (var item in _options.UrisOptions)
                {
                    var method = item.HttpMethod ?? defaultHttpMethod;
                    var expectedCodes = item.ExpectedHttpCodes ?? defaultCodes;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return HealthCheckResult.Failed($"Liveness execution is cancelled.");
                    }

                    using (var httpClient = new HttpClient())
                    {
                        var requestMessage = new HttpRequestMessage(method, item.Uri);

                        foreach (var header in item.Headers)
                        {
                            requestMessage.Headers.Add(header.Name, header.Value);
                        }

                        var response = await httpClient.SendAsync(requestMessage);

                        if (!((int)response.StatusCode >= expectedCodes.Min && (int)response.StatusCode <= expectedCodes.Max))
                        {
                            return HealthCheckResult.Failed($"Discover endpoint #{idx} is not responding with code in {expectedCodes.Min}...{expectedCodes.Max} range, the current status is {response.StatusCode}.");
                        }

                        ++idx;
                    }
                }

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
