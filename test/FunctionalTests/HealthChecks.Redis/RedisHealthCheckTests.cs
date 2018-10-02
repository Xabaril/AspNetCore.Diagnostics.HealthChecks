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

namespace FunctionalTests.HealthChecks.Redis
{
    [Collection("execution")]
    public class redis_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public redis_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_redis_is_available()
        {
            //read appveyor services default values on
            //https://www.appveyor.com/docs/services-databases/#sql-server-2017 

            var connectionString = "localhost:6379,allowAdmin=true";

            var webHostBuilder = new WebHostBuilder()
             .UseStartup<DefaultStartup>()
             .ConfigureServices(services =>
             {
                 services.AddHealthChecks()
                  .AddRedis(connectionString, tags: new string[] { "redis" });
             })
             .Configure(app =>
             {
                 app.UseHealthChecks("/health", new HealthCheckOptions()
                 {
                     Predicate = r => r.Tags.Contains("redis")
                 });
             });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_redis_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
             .UseStartup<DefaultStartup>()
             .ConfigureServices(services =>
             {
                 services.AddHealthChecks()
                  .AddRedis("nonexistinghost:6379,allowAdmin=true", tags: new string[] { "redis" });
             })
             .Configure(app =>
             {
                 app.UseHealthChecks("/health", new HealthCheckOptions()
                 {
                     Predicate = r => r.Tags.Contains("redis")
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
