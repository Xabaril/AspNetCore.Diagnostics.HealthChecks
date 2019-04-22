using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus.Advanced;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Publisher.Prometheus
{
    internal sealed class PrometheusGatewayPublisher : LivenessPrometheusMetrics, IHealthCheckPublisher, IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Uri _targetUrl;

        public PrometheusGatewayPublisher(string endpoint, string job, string instance = null)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            var sb = new StringBuilder($"{endpoint.TrimEnd('/')}/job/{job}");

            if (!string.IsNullOrEmpty(instance))
            {
                sb.AppendFormat("/instance/{0}", instance);
            }

            if (!Uri.TryCreate(sb.ToString(), UriKind.Absolute, out _targetUrl))
            {
                throw new ArgumentException("Endpoint must be a valid url", nameof(endpoint));
            }
        }

        public PrometheusGatewayPublisher(HttpClient httpClient, string endpoint, string job, string instance) 
            : this(endpoint, job, instance)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            WriteMetricsFromHealthReport(report);

            await PushMetrics();
        }

        private async Task PushMetrics()
        {
            try
            {
                using (var outStream = CollectionToStreamWriter(Registry))
                {
                    var response = await _httpClient
                        .PostAsync(_targetUrl, new StreamContent(outStream));

                    response.EnsureSuccessStatusCode();
                }
            }
            catch (ScrapeFailedException ex)
            {
                Trace.WriteLine($"Skipping metrics push due to failed scrape: {ex.Message}");
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Trace.WriteLine($"Error in PushMetrics: {ex.Message}");
            }
        }
    }
}