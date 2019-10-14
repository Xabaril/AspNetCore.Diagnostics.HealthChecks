using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.SignalR
{
    [Collection("execution")]
    public class signalr_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public signalr_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task be_healthy_if_signalr_hub_is_available()
        {
            TestServer server = null;
            var webHostBuilder = new WebHostBuilder()
             .UseStartup<DefaultStartup>()
             .ConfigureServices(services =>
             {
                 services
                    .AddSignalR()
                    .Services
                    .AddHealthChecks()
                    .AddSignalRHub(
                        () => new HubConnectionBuilder()
                                .WithUrl("http://localhost/test", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                                .Build(), 
                        tags: new string[] { "signalr" });
             })
             .Configure(app =>
             {

                 app
                     .UseHealthChecks("/health", new HealthCheckOptions()
                     {
                         Predicate = r => r.Tags.Contains("signalr")
                     })
                     .UseRouting()
                     .UseEndpoints(config =>
                     {
                         config.MapHub<TestHub>("/test");
                     });                     
             });

            server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_signalr_hub_is_unavailable()
        {
            TestServer server = null;
            var webHostBuilder = new WebHostBuilder()
             .UseStartup<DefaultStartup>()
             .ConfigureServices(services =>
             {
                 services
                    .AddSignalR()
                    .Services
                    .AddHealthChecks()
                    .AddSignalRHub(
                        () => new HubConnectionBuilder()
                                .WithUrl("http://localhost/badhub", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                                .Build(),
                        tags: new string[] { "signalr" });
             })
             .Configure(app =>
             {
                 app
                     .UseHealthChecks("/health", new HealthCheckOptions()
                     {
                         Predicate = r => r.Tags.Contains("signalr")
                     })
                     .UseRouting()
                     .UseEndpoints(config =>
                     {
                         config.MapHub<TestHub>("/test");
                     });
             });

            server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        class TestHub : Hub
        {

        }
    }
}
