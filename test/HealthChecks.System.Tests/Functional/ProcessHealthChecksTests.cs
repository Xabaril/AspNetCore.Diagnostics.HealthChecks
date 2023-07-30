using System.Net;

namespace HealthChecks.System.Tests.Functional;

public class process_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_the_process_exists_and_is_running()
    {
        var webhostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddProcessHealthCheck("dotnet", p =>
                    {
                        var proc = p.First();
                        return !proc.HasExited && proc.Responding;
                    });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => true
                });
            });

        using var server = new TestServer(webhostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_the_process_is_not_running()
    {
        var webhostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddProcessHealthCheck("notexist", p => p?.Any() ?? false);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => true
                });
            });

        using var server = new TestServer(webhostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_when_the_predicate_throws_exception()
    {
        var webhostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddProcessHealthCheck("someproc", p => !p.First().HasExited);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => true
                });
            });

        using var server = new TestServer(webhostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
