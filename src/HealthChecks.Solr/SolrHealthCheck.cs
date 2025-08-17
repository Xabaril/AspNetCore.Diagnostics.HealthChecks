using System.Collections.Concurrent;
using HttpWebAdapters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SolrNet.Impl;

namespace HealthChecks.Solr;

public class SolrHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, SolrConnection> _connections = new();

    private readonly SolrOptions _options;

    public SolrHealthCheck(SolrOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            string url = $"{_options.Uri}/{_options.Core}";

            if (!_connections.TryGetValue(url, out var solrConnection))
            {
                solrConnection = new SolrConnection(url)
                {
                    Timeout = (int)_options.Timeout.TotalMilliseconds
                };

                if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password))
                {
                    solrConnection.HttpWebRequestFactory = new BasicAuthHttpWebRequestFactory(_options.Username, _options.Password);
                }

                if (!_connections.TryAdd(url, solrConnection))
                {
                    solrConnection = _connections[url];
                }
            }

            var server = new SolrBasicServer<string>(
                solrConnection,
                queryExecuter: null,
                documentSerializer: null,
                schemaParser: null,
                headerParser: null,
                querySerializer: null,
                dihStatusParser: null,
                extractResponseParser: null);

            var result = await server.PingAsync().ConfigureAwait(false);

            bool isSuccess = result.Status == 0;

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
