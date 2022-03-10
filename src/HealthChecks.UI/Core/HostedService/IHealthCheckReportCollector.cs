namespace HealthChecks.UI.Core.HostedService
{
    internal interface IHealthCheckReportCollector
    {
        Task Collect(CancellationToken cancellationToken);
    }
}
