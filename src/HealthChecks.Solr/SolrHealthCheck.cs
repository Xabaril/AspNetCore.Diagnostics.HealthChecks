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
        private static readonly ConcurrentDictionary<string, ISolrConnection> _connections = new ConcurrentDictionary<string, ISolrConnection>();

        private readonly SolrOptions _options;

        public SolrHealthCheck(SolrOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_connections.TryGetValue(_options.Uri, out ISolrConnection solrConnection))
                {
                    solrConnection = new SolrConnection($"{_options.Uri}/{_options.Core}");

                    if (!_connections.TryAdd(_options.Uri, solrConnection))
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
