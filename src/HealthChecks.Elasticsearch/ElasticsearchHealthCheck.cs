using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace HealthChecks.Elasticsearch
{
    public class ElasticsearchHealthCheck
        : IHealthCheck
    {
        private readonly string _elasticsearchUri;
        public ElasticsearchHealthCheck(string elasticsearchUri)
        {
            _elasticsearchUri = elasticsearchUri ?? throw new ArgumentNullException(nameof(elasticsearchUri));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var lowlevelClient = new ElasticClient(new Uri(_elasticsearchUri));
                var pingResult = await lowlevelClient.PingAsync(cancellationToken: cancellationToken);
                var isSuccess = pingResult.ApiCall.HttpStatusCode == 200;

                return isSuccess
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy(pingResult.ApiCall.DebugInformation, pingResult.ApiCall.OriginalException);

            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
