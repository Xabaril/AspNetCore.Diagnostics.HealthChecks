using Microsoft.Extensions.Diagnostics.HealthChecks;
using SolrNet.Impl;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Solr
{
    public class SolrHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, SolrConnection> _connections = new ConcurrentDictionary<string, SolrConnection>();

        private readonly SolrOptions _options;

        public SolrHealthCheck(SolrOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{_options.Uri}/{_options.Core}";

                if (!_connections.TryGetValue(url, out SolrConnection solrConnection))
                {
                    solrConnection = new SolrConnection(url);
                    solrConnection.Timeout = (int)_options.Timeout.TotalMilliseconds;

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

                var result = await server.PingAsync();

                var isSuccess = result.Status == 0;

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
