using System.Net;

namespace HealthChecks.Consul.Tests.Functional;

public class consul_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_consul_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                   webHostBuilder.Services.AddHealthChecks()
                       .AddConsul(setup =>
                       {
                           setup.HostName = "localhost";
                           setup.Port = 8500;
                           setup.RequireHttps = false;
                       }, tags: ["consul"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                   app.UseHealthChecks("/health", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("consul")
                   });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_consul_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                        .AddConsul(setup =>
                        {
                            setup.HostName = "non-existing-host";
                            setup.Port = 8500;
                            setup.RequireHttps = false;
                        }, tags: ["consul"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("consul")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
