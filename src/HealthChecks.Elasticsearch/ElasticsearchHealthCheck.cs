using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Elasticsearch
{
    public class ElasticsearchHealthCheck
        : IHealthCheck
    {
        private readonly ElasticsearchOptions _options;

        public ElasticsearchHealthCheck(ElasticsearchOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var settings = new ConnectionSettings(new Uri(_options.Uri));

                if (_options.AuthenticateWithBasicCredentials)
                {
                    settings = settings.BasicAuthentication(_options.UserName, _options.Password);
                }
                else if (_options.AuthenticateWithCertificate)
                {
                    settings = settings.ClientCertificate(_options.Certificate);
                }

                var lowlevelClient = new ElasticClient(settings);
                
                var pingResult = await lowlevelClient.PingAsync(cancellationToken: cancellationToken);
                
                var isSuccess = pingResult.ApiCall.HttpStatusCode == 200;

                return isSuccess
                    ? HealthCheckResult.Healthy()
                    : new HealthCheckResult(context.Registration.FailureStatus);

            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
