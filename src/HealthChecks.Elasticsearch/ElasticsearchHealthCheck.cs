using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Elasticsearch
{
    public class ElasticsearchHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ElasticClient> _connections = new ConcurrentDictionary<string, ElasticClient>();

        private readonly ElasticsearchOptions _options;

        public ElasticsearchHealthCheck(ElasticsearchOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_connections.TryGetValue(_options.Uri, out ElasticClient lowLevelClient))
                {
                    var settings = new ConnectionSettings(new Uri(_options.Uri));

                    if (_options.RequestTimeout.HasValue)
                    {
                        settings = settings.RequestTimeout(_options.RequestTimeout.Value);
                    }

                    if (_options.AuthenticateWithBasicCredentials)
                    {
                        settings = settings.BasicAuthentication(_options.UserName, _options.Password);
                    }
                    else if (_options.AuthenticateWithCertificate)
                    {
                        settings = settings.ClientCertificate(_options.Certificate);
                    }

                    if (_options.CertificateValidationCallback != null)
                    {
                        settings = settings.ServerCertificateValidationCallback(_options.CertificateValidationCallback);
                    }

                    lowLevelClient = new ElasticClient(settings);

                    if (!_connections.TryAdd(_options.Uri, lowLevelClient))
                    {
                        lowLevelClient = _connections[_options.Uri];
                    }
                }

                var pingResult = await lowLevelClient.PingAsync(ct: cancellationToken);
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
