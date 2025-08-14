using System.Net;

namespace HealthChecks.Consul.Tests.Functional;

public class consul_healthcheck_should(ConsulContainerFixture consulFixture) : IClassFixture<ConsulContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_consul_is_available()
    {
        var options = consulFixture.GetConnectionOptions();

        var webHostBuilder = new WebHostBuilder()
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                   .AddConsul(setup =>
                   {
                       setup.HostName = options.HostName;
                       setup.Port = options.Port;
                       setup.RequireHttps = options.RequireHttps;
                   }, tags: ["consul"]);
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions
               {
                   Predicate = r => r.Tags.Contains("consul")
               });
           });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_consul_is_not_available()
    {
        var options = consulFixture.GetConnectionOptions();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddConsul(setup =>
                    {
                        setup.HostName = "non-existing-host";
                        setup.Port = options.Port;
                        setup.RequireHttps = options.RequireHttps;
                    }, tags: ["consul"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("consul")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
