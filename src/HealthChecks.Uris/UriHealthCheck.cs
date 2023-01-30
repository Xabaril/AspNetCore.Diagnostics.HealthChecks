using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Uris;

public class UriHealthCheck : IHealthCheck
{
    private readonly UriHealthCheckOptions _options;
    private readonly Func<HttpClient> _httpClientFactory;

    public UriHealthCheck(UriHealthCheckOptions options, Func<HttpClient> httpClientFactory)
    {
        _options = Guard.ThrowIfNull(options);
        _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var defaultHttpMethod = _options.HttpMethod;
        var defaultExpectedStatusCodes = _options.ExpectedHttpCodes;
        var defaultTimeout = _options.Timeout;
        int idx = 0;

        try
        {
            foreach (var item in _options.UrisOptions)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"{nameof(UriHealthCheck)} execution is cancelled.");
                }

                var method = item.HttpMethod ?? defaultHttpMethod;
                var (Min, Max) = item.ExpectedHttpCodes ?? defaultExpectedStatusCodes;
                var timeout = item.Timeout != TimeSpan.Zero ? item.Timeout : defaultTimeout;

                var httpClient = _httpClientFactory();

                using var requestMessage = new HttpRequestMessage(method, item.Uri);

#if NET5_0_OR_GREATER
                requestMessage.Version = httpClient.DefaultRequestVersion;
                requestMessage.VersionPolicy = httpClient.DefaultVersionPolicy;
#endif

                foreach (var (Name, Value) in item.Headers)
                {
                    requestMessage.Headers.Add(Name, Value);
                }

                using (var timeoutSource = new CancellationTokenSource(timeout))
                using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken))
                {
                    using var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, linkedSource.Token).ConfigureAwait(false);

                    if (!((int)response.StatusCode >= Min && (int)response.StatusCode <= Max))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint #{idx} is not responding with code in {Min}...{Max} range, the current status is {response.StatusCode}.");
                    }

                    if (item.ExpectedContent != null)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (responseBody != item.ExpectedContent)
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"The expected value '{item.ExpectedContent}' was not found in the response body.");
                    }

                    ++idx;
                }
            }
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
