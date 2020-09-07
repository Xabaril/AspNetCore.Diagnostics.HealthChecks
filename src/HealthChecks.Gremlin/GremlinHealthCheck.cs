using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

using static Gremlin.Net.Process.Traversal.AnonymousTraversalSource;

namespace HealthChecks.Gremlin
{
    public class GremlinHealthCheck : IHealthCheck
    {
        protected readonly GremlinServer _server;

        public GremlinHealthCheck(GremlinOptions options)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _ = options.Hostname ?? throw new ArgumentNullException(nameof(options.Hostname));

            _server = new GremlinServer(options.Hostname, options.Port, options.EnableSsl);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = new GremlinClient(_server))
                using (var conn = new DriverRemoteConnection(client))
                {
                    var g = Traversal().WithRemote(conn);
                    await g.Inject(0).Promise(t => t.Next());
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
