namespace HealthChecks.UI.Core.HostedService;

public interface IHealthCheckReportCollector
{
    Task Collect(CancellationToken cancellationToken);
}
