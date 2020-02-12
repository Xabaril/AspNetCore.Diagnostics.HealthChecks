using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SolrNet;
using SolrNet.Impl;
using SolrNet.Impl.ResponseParsers;
using SolrNet.Commands;

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
                    solrConnection.Timeout = _options.Timeout;

                    if (!_connections.TryAdd(url, solrConnection))
                    {
                        solrConnection = _connections[_options.Uri];
                    }
                }

                var server = new SolrBasicServer<string>(solrConnection, null, null, null, null, null, null, null);
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
