using System.Net;

namespace HealthChecks.Redis.Tests.Functional
{
    public class redis_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_redis_is_available()
        {
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

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_multiple_redis_are_available()
        {
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

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
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

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_if_redis_is_not_available_within_specified_timeout()
        {
            var webHostBuilder = new WebHostBuilder()
             .ConfigureServices(services =>
             {
                 services.AddHealthChecks()
                  .AddRedis("nonexistinghost:6379,allowAdmin=true,connectRetry=2147483647", tags: new string[] { "redis" }, timeout: TimeSpan.FromSeconds(2));
             })
             .Configure(app =>
             {
                 app.UseHealthChecks("/health", new HealthCheckOptions
                 {
                     Predicate = r => r.Tags.Contains("redis")
                 });
             });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

            response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
            (await response.Content.ReadAsStringAsync().ConfigureAwait(false)).ShouldContain("Healthcheck timed out");
        }
    }
}
