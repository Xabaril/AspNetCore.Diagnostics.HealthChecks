using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Elasticsearch
{
    public abstract class BaseElasticsearchHealthCheck
         : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ElasticClient> _connections = new ConcurrentDictionary<string, ElasticClient>();

        private readonly ElasticsearchOptions _options;

        public BaseElasticsearchHealthCheck(ElasticsearchOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public abstract Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);

        protected ElasticClient GenerateClient()
        {
            if (!_connections.TryGetValue(_options.Uri, out ElasticClient lowLevelClient))
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

            return lowLevelClient;
        }
    }
}