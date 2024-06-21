using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using Elasticsearch.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;

namespace HealthChecks.Elasticsearch;

public class ElasticsearchHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, ElasticClient> _connections = new();

    private readonly ElasticsearchOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "healthcheck.name", nameof(ElasticsearchHealthCheck) },
                    { "healthcheck.task", "online" },
                    { "db.system", "elasticsearch" },
                    { "event.name", "database.healthcheck"},
                    { "client.address", Dns.GetHostName()},
                    { "network.protocol.name", "http" },
                    { "network.transport", "tcp" }
    };

    public ElasticsearchHealthCheck(ElasticsearchOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            if (!_connections.TryGetValue(_options.Uri, out var lowLevelClient))
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
                else if (_options.AuthenticateWithApiKey)
                {
                    settings = settings.ApiKeyAuthentication(_options.ApiKeyAuthenticationCredentials);
                }

                if (_options.CertificateValidationCallback != null)
                {
                    settings = settings.ServerCertificateValidationCallback(_options.CertificateValidationCallback);
                }

                lowLevelClient = new ElasticClient(settings);

                if (!_connections.TryAdd(_options.Uri, lowLevelClient))
                {
                    checkDetails.Add("server.address", _options.Uri);
                    lowLevelClient = _connections[_options.Uri];
                }
            }

            if (_options.UseClusterHealthApi)
            {
                checkDetails.Add("healthcheck.task", "ready");
                var healthResponse = await lowLevelClient.Cluster.HealthAsync(ct: cancellationToken).ConfigureAwait(false);

                if (healthResponse.ApiCall.HttpStatusCode != 200)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, data: new ReadOnlyDictionary<string, object>(checkDetails));
                }

                return healthResponse.Status switch
                {
                    Health.Green => HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails)),
                    Health.Yellow => HealthCheckResult.Degraded(data: new ReadOnlyDictionary<string, object>(checkDetails)),
                    _ => new HealthCheckResult(context.Registration.FailureStatus, data: new ReadOnlyDictionary<string, object>(checkDetails))
                };
            }
            checkDetails.Add("healthcheck.task", "online");
            var pingResult = await lowLevelClient.PingAsync(ct: cancellationToken).ConfigureAwait(false);
            bool isSuccess = pingResult.ApiCall.HttpStatusCode == 200;

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
