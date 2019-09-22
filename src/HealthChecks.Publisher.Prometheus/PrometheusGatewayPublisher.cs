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
    internal sealed class PrometheusGatewayPublisher : LivenessPrometheusMetrics, IHealthCheckPublisher
    {
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly Uri _targetUrl;

        public PrometheusGatewayPublisher(Func<HttpClient> httpClientFactory, string endpoint, string job, string instance = null)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

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
                    var response = await _httpClientFactory()
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