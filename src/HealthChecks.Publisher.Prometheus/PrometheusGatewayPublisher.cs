using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

namespace HealthChecks.Publisher.Prometheus;

internal sealed class PrometheusGatewayPublisher : LivenessPrometheusMetrics, IHealthCheckPublisher
{
    private readonly Func<HttpClient> _httpClientFactory;
    private readonly Uri _targetUrl;

    public PrometheusGatewayPublisher(Func<HttpClient> httpClientFactory, string endpoint, string job, string? instance = null)
    {
        _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);

        Guard.ThrowIfNull(endpoint);

        var sb = new StringBuilder($"{endpoint.TrimEnd('/')}/job/{job}");

        if (!string.IsNullOrEmpty(instance))
        {
            sb.AppendFormat("/instance/{0}", instance);
        }

        _targetUrl = Uri.TryCreate(sb.ToString(), UriKind.Absolute, out var temp)
            ? temp
            : throw new ArgumentException("Endpoint must be a valid url", nameof(endpoint));
    }

    public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        WriteMetricsFromHealthReport(report);

        await PushMetricsAsync().ConfigureAwait(false);
    }

    private async Task PushMetricsAsync()
    {
        try
        {
            using var outStream = new MemoryStream();

            await Registry.CollectAndExportAsTextAsync(outStream).ConfigureAwait(false);
            outStream.Position = 0;

            using var request = new HttpRequestMessage(HttpMethod.Post, _targetUrl)
            {
                Content = new StreamContent(outStream)
            };

            using var response = await _httpClientFactory()
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }
        catch (ScrapeFailedException ex)
        {
            Trace.WriteLine($"Skipping metrics push due to failed scrape: {ex.Message}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Trace.WriteLine($"Error in PushMetrics: {ex.Message}");
        }
    }
}
