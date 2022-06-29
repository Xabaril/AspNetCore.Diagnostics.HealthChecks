using System.Net;

namespace HealthChecks.Redis.Tests.Functional
{
    public class redis_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_redis_is_available()
        {
            //read appveyor services default values on
            //https://www.appveyor.com/docs/services-databases/#sql-server-2017

            var connectionString = "localhost:6379,allowAdmin=true";

            var webHostBuilder = new WebHostBuilder()
             .ConfigureServices(services =>
             {
                 services.AddHealthChecks()
                  .AddRedis(connectionString, tags: new string[] { "redis" });
             })
             .Configure(app =>
             {
                 app.UseHealthChecks("/health", new HealthCheckOptions
                 {
                     Predicate = r => r.Tags.Contains("redis")
                 });
             });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_multiple_redis_are_available()
        {
            //read appveyor services default values on
            //https://www.appveyor.com/docs/services-databases/#sql-server-2017

            var connectionString = "localhost:6379,allowAdmin=true";

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddRedis(connectionString, tags: new string[] { "redis" }, name: "1")
                    .AddRedis(connectionString, tags: new string[] { "redis" }, name: "2");
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("redis")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_redis_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
             .ConfigureServices(services =>
             {
                 services.AddHealthChecks()
                  .AddRedis("nonexistinghost:6379,allowAdmin=true", tags: new string[] { "redis" });
             })
             .Configure(app =>
             {
                 app.UseHealthChecks("/health", new HealthCheckOptions
                 {
                     Predicate = r => r.Tags.Contains("redis")
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
