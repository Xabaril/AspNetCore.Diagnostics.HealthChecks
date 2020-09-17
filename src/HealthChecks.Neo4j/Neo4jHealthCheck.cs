using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace HealthChecks.Neo4j
{
    public class Neo4jHealthCheck : IHealthCheck
    {
        private readonly Neo4jOptions _options;

        public Neo4jHealthCheck(Neo4jOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var driver = GraphDatabase.Driver(_options.Uri, AuthTokens.Basic(_options.UserName, _options.Password)))
                {
                    var session = driver.AsyncSession();
                    var reader = await session.RunAsync("MATCH (n) RETURN count(n) as count");
                    var fetched = await reader.FetchAsync();

                    if (!fetched)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Impossible to fetch data from a database.");
                    }

                    await session.CloseAsync();
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}