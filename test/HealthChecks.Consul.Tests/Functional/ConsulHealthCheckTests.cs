using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace HealthChecks.Consul.Tests.Functional
{
    public class consul_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_consul_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                       .AddConsul(setup =>
                       {
                           setup.HostName = "localhost";
                           setup.Port = 8500;
                           setup.RequireHttps = false;
                       }, tags: new string[] { "consul" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("consul")
                   });
               });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_consul_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddConsul(setup =>
                        {
                            setup.HostName = "non-existing-host";
                            setup.Port = 8500;
                            setup.RequireHttps = false;
                        }, tags: new string[] { "consul" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("consul")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
