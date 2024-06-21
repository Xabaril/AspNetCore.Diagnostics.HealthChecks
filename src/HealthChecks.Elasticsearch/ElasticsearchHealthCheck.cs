using System.Collections.Concurrent;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Elasticsearch;

public class ElasticsearchHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, ElasticsearchClient> _connections = new();

    private readonly ElasticsearchOptions _options;

    public ElasticsearchHealthCheck(ElasticsearchOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connections.TryGetValue(_options.Uri, out var elasticsearchClient))
            {
                var settings = new ElasticsearchClientSettings(new Uri(_options.Uri));

                if (_options.RequestTimeout.HasValue)
                {
                    settings = settings.RequestTimeout(_options.RequestTimeout.Value);
                }

                if (_options.AuthenticateWithBasicCredentials)
                {
                    if (_options.UserName is null)
                    {
                        throw new ArgumentNullException(nameof(_options.UserName));
                    }
                    if (_options.Password is null)
                    {
                        throw new ArgumentNullException(nameof(_options.Password));
                    }
                    settings = settings.Authentication(new BasicAuthentication(_options.UserName, _options.Password));
                }
                else if (_options.AuthenticateWithCertificate)
                {
                    if (_options.Certificate is null)
                    {
                        throw new ArgumentNullException(nameof(_options.Certificate));
                    }
                    settings = settings.ClientCertificate(_options.Certificate);
                }
                else if (_options.AuthenticateWithApiKey)
                {
                    if (_options.ApiKey is null)
                    {
                        throw new ArgumentNullException(nameof(_options.ApiKey));
                    }
                    settings.Authentication(new ApiKey(_options.ApiKey));
                }

                if (_options.CertificateValidationCallback != null)
                {
                    settings = settings.ServerCertificateValidationCallback(_options.CertificateValidationCallback);
                }

                elasticsearchClient = new ElasticsearchClient(settings);

                if (!_connections.TryAdd(_options.Uri, elasticsearchClient))
                {
                    elasticsearchClient = _connections[_options.Uri];
                }
            }

            if (_options.UseClusterHealthApi)
            {
                var healthResponse = await elasticsearchClient.Cluster.HealthAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (healthResponse.ApiCallDetails.HttpStatusCode != 200)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus);
                }

                return healthResponse.Status switch
                {
                    Elastic.Clients.Elasticsearch.HealthStatus.Green => HealthCheckResult.Healthy(),
                    Elastic.Clients.Elasticsearch.HealthStatus.Yellow => HealthCheckResult.Degraded(),
                    _ => new HealthCheckResult(context.Registration.FailureStatus)
                };
            }

            var pingResult = await elasticsearchClient.PingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            bool isSuccess = pingResult.ApiCallDetails.HttpStatusCode == 200;

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
