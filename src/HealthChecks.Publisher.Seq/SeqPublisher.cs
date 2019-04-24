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
        private const string EVENT_NAME = "AspNetCoreHealthCheckResult";
        private const string METRIC_STATUS_NAME = "AspNetCoreHealthCheckStatus";
        private const string METRIC_DURATION_NAME = "AspNetCoreHealthCheckDuration";

        private readonly SeqOptions _options;
        private readonly Func<HttpClient> _httpClientFactory;

        public SeqPublisher(Func<HttpClient> httpClientFactory, SeqOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }
        public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            var level = "Information";
            switch (report.Status)
            {
                case HealthStatus.Degraded:
                    level = "Warning";
                    break;
                case HealthStatus.Unhealthy:
                    level = "Error";
                    break;
            }

            var events = new RawEvents
            {
                Events = new RawEvent[]
                {
                    new RawEvent
                    {
                        Timestamp = DateTimeOffset.UtcNow,
                        MessageTemplate = EVENT_NAME,
                        Level = level,
                        Properties = new Dictionary<string, object>
                        {
                            { nameof(Environment.MachineName), Environment.MachineName },
                            { nameof(Assembly), Assembly.GetEntryAssembly().GetName().Name },
                            { METRIC_STATUS_NAME, (int)report.Status },
                            { METRIC_DURATION_NAME, report.TotalDuration.TotalMilliseconds }
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
            public Dictionary<string, object> Properties { get; set; }
        }
    }
}
