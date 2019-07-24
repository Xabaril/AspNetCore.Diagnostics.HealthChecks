using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.EventStore
{
    [Collection("execution")]
    public class eventstore_healthcheck_connectionString_should
    {
        private readonly ExecutionFixture _fixture;

        public eventstore_healthcheck_connectionString_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_eventstore_is_available()
        {
            var connectionString = "ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500";

            var webHostBuilder = new WebHostBuilder()
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddEventStoreConnectionString(connectionString, tags: new string[] { "eventstore" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("eventstore")
                });
            });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_if_eventstore_is_not_available()
        {
            var connectionString = "ConnectTo=tcp://admin:changeit@nonexistingdomain:1113; HeartBeatTimeout=500";

            var webHostBuilder = new WebHostBuilder()
           .UseStartup<DefaultStartup>()
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                 .AddEventStoreConnectionString(connectionString, tags: new string[] { "eventstore" });
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions()
               {
                   Predicate = r => r.Tags.Contains("eventstore")
               });
           });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
