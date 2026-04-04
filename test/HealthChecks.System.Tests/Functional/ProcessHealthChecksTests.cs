using System.Net;

namespace HealthChecks.System.Tests.Functional;

public class process_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_the_process_exists_and_is_running()
    {
        var webhostBuilder = WebApplication.CreateBuilder();
                    webhostBuilder.Services.AddHealthChecks()
                        .AddProcessHealthCheck("dotnet", p =>
                        {
                            var proc = p.First();
                            return !proc.HasExited && proc.Responding;
                        });
        webhostBuilder.WebHost.UseTestServer();
        await using var app = webhostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => true
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_the_process_is_not_running()
    {
        var webhostBuilder = WebApplication.CreateBuilder();
                    webhostBuilder.Services.AddHealthChecks()
                        .AddProcessHealthCheck("notexist", p => p?.Any() ?? false);
        webhostBuilder.WebHost.UseTestServer();
        await using var app = webhostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => true
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_when_the_predicate_throws_exception()
    {
        var webhostBuilder = WebApplication.CreateBuilder();
                    webhostBuilder.Services.AddHealthChecks()
                        .AddProcessHealthCheck("someproc", p => !p.First().HasExited);
        webhostBuilder.WebHost.UseTestServer();
        await using var app = webhostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => true
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
