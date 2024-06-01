using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Elasticsearch;

public class ElasticsearchHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, ElasticsearchClient> _connections = new();

    private readonly ElasticsearchOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.name", nameof(ElasticsearchHealthCheck) },
                    { "health_check.task", "online" },
                    { "db.system.name", "elasticsearch" }
    };

    public ElasticsearchHealthCheck(ElasticsearchOptions options)
    {
        Debug.Assert(options.Uri is not null || options.Client is not null || options.AuthenticateWithElasticCloud);
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            ElasticsearchClient? elasticsearchClient = null;
            if (_options.Client is not null)
            {
                elasticsearchClient = _options.Client;
            }
            else
            {
                ElasticsearchClientSettings? settings = null;

                settings = _options.AuthenticateWithElasticCloud
               ? new ElasticsearchClientSettings(_options.CloudId!, new ApiKey(_options.ApiKey!))
               : new ElasticsearchClientSettings(new Uri(_options.Uri!));


                if (_options.RequestTimeout.HasValue)
                {
                    settings = settings.RequestTimeout(_options.RequestTimeout.Value);
                }

                if (!_connections.TryGetValue(_options.Uri!, out elasticsearchClient))
                {

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

                    if (!_connections.TryAdd(_options.Uri!, elasticsearchClient))
                    {
                        elasticsearchClient = _connections[_options.Uri!];
                    }
                    checkDetails.Add("server.address", _options.Uri);
                }
            }

            if (_options.UseClusterHealthApi)
            {
                checkDetails.Add("health_check.task", "ready");
                var healthResponse = await elasticsearchClient.Cluster.HealthAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (healthResponse.ApiCallDetails.HttpStatusCode != 200)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, data: new ReadOnlyDictionary<string, object>(checkDetails));
                }

                return healthResponse.Status switch
                {
                    Elastic.Clients.Elasticsearch.HealthStatus.Green => HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails)),
                    Elastic.Clients.Elasticsearch.HealthStatus.Yellow => HealthCheckResult.Degraded(data: new ReadOnlyDictionary<string, object>(checkDetails)),
                    _ => new HealthCheckResult(context.Registration.FailureStatus, data: new ReadOnlyDictionary<string, object>(checkDetails))
                };
            }

            checkDetails.Add("health_check.task", "online");
            var pingResult = await elasticsearchClient.PingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            bool isSuccess = pingResult.ApiCallDetails.HttpStatusCode == 200;

            return isSuccess
                ? HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails))
                : new HealthCheckResult(context.Registration.FailureStatus, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
