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
        private readonly TimeSpan _timeout;

        public ElasticsearchHealthCheck(ElasticsearchOptions options, TimeSpan timeout)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
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

                    var pingResult = await lowlevelClient.PingAsync(cancellationToken: timeoutCancellationTokenSource.Token);

                    var isSuccess = pingResult.ApiCall.HttpStatusCode == 200;

                    return isSuccess
                        ? HealthCheckResult.Healthy()
                        : new HealthCheckResult(context.Registration.FailureStatus);

                }
                catch (Exception ex)
                {
                    if (timeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Timeout");
                    }
                    return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                }
            }
        }
    }
}
