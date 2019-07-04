using Elasticsearch.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Elasticsearch
{
    public class ElasticsearchClusterHealthCheck
       : BaseElasticsearchHealthCheck
    {
        public ElasticsearchClusterHealthCheck(ElasticsearchOptions options) : base(options) { }

        public override async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                ElasticClient lowLevelClient = GenerateClient();

                ClusterHealthDescriptor clusterHealthDescriptor = new ClusterHealthDescriptor();

                var clusterResult = await lowLevelClient
                    .Cluster
                    .HealthAsync(selector: c => clusterHealthDescriptor, ct: cancellationToken);

                var isSuccessfulCall = clusterResult.ApiCall.HttpStatusCode == 200;

                if (!isSuccessfulCall)
                {
                    return new HealthCheckResult(
                        context.Registration.FailureStatus,
                        description: $"Elastic Search responded with status: '{clusterResult.ApiCall.HttpStatusCode}'. {clusterResult.ApiCall.DebugInformation}");
                }

                switch (clusterResult.Status)
                {
                    case Health.Green:
                        return HealthCheckResult.Healthy();
                    case Health.Yellow:
                        return HealthCheckResult.Degraded();
                    case Health.Red:
                    default:
                        return HealthCheckResult.Unhealthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
