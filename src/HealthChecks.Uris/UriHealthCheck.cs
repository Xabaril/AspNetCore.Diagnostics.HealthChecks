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
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly TimeSpan _timeout;

        public UriHealthCheck(UriHealthCheckOptions options, Func<HttpClient> httpClientFactory, TimeSpan timeout)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var defaultHttpMethod = _options.HttpMethod;
            var defaultCodes = _options.ExpectedHttpCodes;
            var idx = 0;

            foreach (var item in _options.UrisOptions)
            {
                var method = item.HttpMethod ?? defaultHttpMethod;
                var expectedCodes = item.ExpectedHttpCodes ?? defaultCodes;

                if (cancellationToken.IsCancellationRequested)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"{nameof(UriHealthCheck)} execution is cancelled.");
                }

                using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
                using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
                {
                    var httpClient = _httpClientFactory();
                    try
                    {
                        using (var requestMessage = new HttpRequestMessage(method, item.Uri))
                        {

                            foreach (var header in item.Headers)
                            {
                                requestMessage.Headers.Add(header.Name, header.Value);
                            }

                            using (var response = await httpClient.SendAsync(requestMessage, timeoutCancellationTokenSource.Token))
                            {
                                if (!((int)response.StatusCode >= expectedCodes.Min && (int)response.StatusCode <= expectedCodes.Max))
                                {
                                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint #{idx} is not responding with code in {expectedCodes.Min}...{expectedCodes.Max} range, the current status is {response.StatusCode}.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (timeoutCancellationTokenSource.IsCancellationRequested)
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, $"Discover endpoint #{idx} timed out");
                        }
                        return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                    }
                }

                ++idx;
            }

            return HealthCheckResult.Healthy();
        }
    }
}
