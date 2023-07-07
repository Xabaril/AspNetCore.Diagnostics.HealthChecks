namespace HealthChecks.ApplicationStatus.Tests.Functional;

public class applicationstatus_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_application_is_not_stopped()
    {
        var sut = new ApplicationStatusHealthCheck(new TestHostApplicationLifeTime());
        var result = await sut.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
        result.ShouldBe(HealthCheckResult.Healthy());
    }

    [Fact]
    public async Task be_unhealthy_if_application_is_stopped()
    {
        var lifetime = new TestHostApplicationLifeTime();
        var sut = new ApplicationStatusHealthCheck(lifetime);
        lifetime.StopApplication();

        var result = await sut.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);

        result.ShouldBe(HealthCheckResult.Unhealthy());
    }
}
