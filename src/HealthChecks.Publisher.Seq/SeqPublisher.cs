using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                        MessageTemplate = $"[{Assembly.GetEntryAssembly().GetName().Name} - HealthCheck Result]",
                        Level = level.ToString(),
                        Properties = new Dictionary<string, object>
                        {
                            { nameof(Environment.MachineName), Environment.MachineName },
                            { nameof(Assembly), Assembly.GetEntryAssembly().GetName().Name },
                            { "Status", report.Status.ToString() },
                            { "TimeElapsed", report.TotalDuration.TotalMilliseconds },
                            { "RawReport" , JsonConvert.SerializeObject(report)}
                        }
                    }
                }
            };

            await PushMetrics(JsonConvert.SerializeObject(events));
        }
        private async Task PushMetrics(string json)
        {
            try
            {
                var httpClient = _httpClientFactory();

                var pushMessage = new HttpRequestMessage(HttpMethod.Post, $"{_options.Endpoint}/api/events/raw?apiKey={_options.ApiKey}");
                pushMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                (await httpClient.SendAsync(pushMessage))
                    .EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception is throwed publishing metrics to Seq with message: {ex.Message}");
            }
        }
        private class RawEvents
        {
            public RawEvent[] Events { get; set; }
        }
        private class RawEvent
        {
            public DateTimeOffset Timestamp { get; set; }
            public string Level { get; set; }
            public string MessageTemplate { get; set; }
            public string RawReport { get; set; }
            public Dictionary<string, object> Properties { get; set; }
        }
    }
}
