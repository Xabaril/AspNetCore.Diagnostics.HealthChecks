using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Publisher.Seq;

public class SeqPublisher : IHealthCheckPublisher
{
    private readonly SeqOptions _options;
    private readonly Func<HttpClient> _httpClientFactory;
    private readonly Uri _checkUri;

    public SeqPublisher(Func<HttpClient> httpClientFactory, SeqOptions options)
    {
        _options = Guard.ThrowIfNull(options);
        _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
        _checkUri = BuildCheckUri(options);
    }

    public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        var level = _options.DefaultInputLevel;

        switch (report.Status)
        {
            case HealthStatus.Degraded:
                level = SeqInputLevel.Warning;
                break;
            case HealthStatus.Unhealthy:
                level = SeqInputLevel.Error;
                break;
        }

        string? assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;

        var events = new RawEvents
        {
            Events =
            [
                new RawEvent
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    MessageTemplate = $"[{assemblyName} - HealthCheck Result]",
                    Level = level.ToString(),
                    Properties = new Dictionary<string, object?>
                    {
                        { nameof(Environment.MachineName), Environment.MachineName },
                        { nameof(Assembly), assemblyName },
                        { "Status", report.Status.ToString() },
                        { "TimeElapsed", report.TotalDuration.TotalMilliseconds },
                        { "RawReport" , JsonSerializer.Serialize(report)}
                    }
                }
            ]
        };

        _options.Configure?.Invoke(events);

        await PushMetricsAsync(JsonSerializer.Serialize(events), cancellationToken).ConfigureAwait(false);
    }

    private async Task PushMetricsAsync(string json, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory();

            using var pushMessage = new HttpRequestMessage(HttpMethod.Post, _checkUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var response = await httpClient.SendAsync(
                pushMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Exception thrown publishing metrics to Seq with message: {ex.Message}");
        }
    }

    private static Uri BuildCheckUri(SeqOptions options)
    {
        Guard.ThrowIfNull(options.Endpoint, true);

        var uriBuilder = new UriBuilder(options.Endpoint)
        {
            Path = "/api/events/raw",
        };

        // Add api key if supplied
        if (!string.IsNullOrEmpty(options.ApiKey))
            uriBuilder.Query = "?apiKey=" + options.ApiKey;

        return uriBuilder.Uri;
    }

}
