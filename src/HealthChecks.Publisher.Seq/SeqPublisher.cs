using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace HealthChecks.Publisher.Seq
{
    public class SeqPublisher : IHealthCheckPublisher
    {

        private readonly SeqOptions _options;
        private readonly Func<HttpClient> _httpClientFactory;

        public SeqPublisher(Func<HttpClient> httpClientFactory, SeqOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
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

            var events = new RawEvents
            {
                Events = new RawEvent[]
                {
                    new RawEvent
                    {
                        Timestamp = DateTimeOffset.UtcNow,
                        MessageTemplate = $"[{Assembly.GetEntryAssembly()?.GetName().Name} - HealthCheck Result]",
                        Level = level.ToString(),
                        Properties = new Dictionary<string, object?>
                        {
                            { nameof(Environment.MachineName), Environment.MachineName },
                            { nameof(Assembly), Assembly.GetEntryAssembly()?.GetName().Name },
                            { "Status", report.Status.ToString() },
                            { "TimeElapsed", report.TotalDuration.TotalMilliseconds },
                            { "RawReport" , JsonConvert.SerializeObject(report)}
                        }
                    }
                }
            };

            await PushMetricsAsync(JsonConvert.SerializeObject(events));
        }

        private async Task PushMetricsAsync(string json)
        {
            try
            {
                var httpClient = _httpClientFactory();

                using var pushMessage = new HttpRequestMessage(HttpMethod.Post, $"{_options.Endpoint}/api/events/raw?apiKey={_options.ApiKey}")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                using var response = await httpClient.SendAsync(pushMessage, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception is throwed publishing metrics to Seq with message: {ex.Message}");
            }
        }

        private class RawEvents
        {
            public RawEvent[] Events { get; set; } = null!;
        }

        private class RawEvent
        {
            public DateTimeOffset Timestamp { get; set; }

            public string Level { get; set; } = null!;

            public string MessageTemplate { get; set; } = null!;

            public string RawReport { get; set; } = null!; //TODO: remove?

            public Dictionary<string, object?> Properties { get; set; } = null!;
        }
    }
}
