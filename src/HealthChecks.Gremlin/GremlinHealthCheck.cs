using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using static Gremlin.Net.Process.Traversal.AnonymousTraversalSource;

namespace HealthChecks.Gremlin;

public class GremlinHealthCheck : IHealthCheck
{
    protected readonly GremlinServer _server;

    public GremlinHealthCheck(GremlinOptions options)
    {
        _ = Guard.ThrowIfNull(options);
        _ = Guard.ThrowIfNull(options.Hostname);

        _server = new GremlinServer(options.Hostname, options.Port, options.EnableSsl);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new GremlinClient(_server);
            using var conn = new DriverRemoteConnection(client);
            var g = Traversal().WithRemote(conn);
#pragma warning disable IDISP004 // Don't ignore created IDisposable
            await g.Inject(0).Promise(t => t.Next()).ConfigureAwait(false);
#pragma warning restore IDISP004 // Don't ignore created IDisposable
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
        }

        return HealthCheckResult.Healthy();
    }
}
