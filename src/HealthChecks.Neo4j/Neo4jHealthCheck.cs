using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neo4j.Driver;

public class Neo4jHealthCheck : IHealthCheck
{
    private readonly IDriver _driver;

    public Neo4jHealthCheck(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _driver.ExecutableQuery("RETURN 1")
              .ExecuteAsync(cancellationToken)
              .ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
