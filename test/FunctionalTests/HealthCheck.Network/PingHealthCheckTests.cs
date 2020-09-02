using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.Network;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;


namespace FunctionalTests.HealthChecks.Network
{
    [Collection("execution")]
    public class ping_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public ping_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task be_healthy_when_all_hosts_reply_to_ping()
        {
            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddPingHealthCheck(setup =>
                    {
                        setup.AddHost("127.0.0.1", 5000);
                    }, tags: new string[] { "ping" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("ping")
                   });
               });


            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
              .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_unhealthy_when_a_host_ping_is_not_successful()
        {
            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddPingHealthCheck(setup =>
                    {
                        setup.AddHost("127.0.0.1", 3000);
                        setup.AddHost("nonexistinghost", 3000);
                    }, tags: new string[] { "ping" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("ping")
                   });
               });


            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
              .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
