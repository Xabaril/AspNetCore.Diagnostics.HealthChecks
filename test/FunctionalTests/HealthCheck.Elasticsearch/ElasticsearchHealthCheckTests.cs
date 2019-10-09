using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.Elasticsearch
{
    [Collection("execution")]
    public class elasticsearch_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public elasticsearch_healthcheck_should(ExecutionFixture fixture)
        { 
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_elasticsearch_is_available()
        {
            var connectionString = @"http://localhost:9200";

            var webHostBuilder = new WebHostBuilder()
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddElasticsearch(connectionString, tags: new string[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
            });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_if_elasticsearch_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
           .UseStartup<DefaultStartup>()
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                .AddElasticsearch("nonexistingdomain:9200", tags: new string[] { "elasticsearch" });
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions()
               {
                   Predicate = r => r.Tags.Contains("elasticsearch")
               });
           });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_if_elasticsearch_cluster_health_status_is_red()
        {
            await RunWithMockElasticsearch("red", async server =>
            {
                var response = await server.CreateRequest("/health").GetAsync();

                var body = await response.Content.ReadAsStringAsync();

                response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

                body.Should().Be("Unhealthy");
            });
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_elasticsearch_cluster_health_status_is_green()
        {
            await RunWithMockElasticsearch("green", async server =>
            {
                var response = await server.CreateRequest("/health").GetAsync();

                var body = await response.Content.ReadAsStringAsync();

                body.Should().Be("Healthy");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            });
        }

        [SkipOnAppVeyor]
        public async Task be_degraded_if_elasticsearch_cluster_health_status_is_yellow()
        {
            await RunWithMockElasticsearch("yellow", async server =>
            {
                var response = await server.CreateRequest("/health").GetAsync();

                var body = await response.Content.ReadAsStringAsync();

                body.Should().Be("Degraded");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            });
        }

        private async Task RunWithMockElasticsearch(string clusterStatus, Func<TestServer, Task> action)
        {
            string clusterHealthJsonResponse = @"{{
  ""cluster_name"" : ""docker-cluster"",
  ""status"" : ""{0}"",
  ""timed_out"" : false,
  ""number_of_nodes"" : 1,
  ""number_of_data_nodes"" : 1,
  ""active_primary_shards"" : 12,
  ""active_shards"" : 12,
  ""relocating_shards"" : 0,
  ""initializing_shards"" : 0,
  ""unassigned_shards"" : 0,
  ""delayed_unassigned_shards"" : 0,
  ""number_of_pending_tasks"" : 0,
  ""number_of_in_flight_fetch"" : 0,
  ""task_max_waiting_in_queue_millis"" : 0,
  ""active_shards_percent_as_number"" : 100.00
}}";
            var port = new Random().Next(19200, 19299);

            var elasticsearchUrl = $"http://localhost:{port}";

            var elasticsearchHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .UseKestrel()
                .UseUrls(elasticsearchUrl)
                .Configure(app =>
                {
                    var response = string.Format(clusterHealthJsonResponse, clusterStatus);
                    app.Map("/_cluster/health", c => c.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK; // Elasticsearch always return 200 OK for this API
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsync(response);
                    }));
                });

            using (var elastichsearchHost = elasticsearchHostBuilder.Build())
            {
                await elastichsearchHost.StartAsync();

                var webHostBuilder = new WebHostBuilder()
                    .UseStartup<DefaultStartup>()
                    .ConfigureServices(services =>
                    {
                        services.AddHealthChecks()
                            .AddElasticsearch(setup =>
                            {
                                setup.UseServer(elasticsearchUrl)
                                    .UseClusterHealth();
                            }, tags: new string[] { "elasticsearch" });
                    })
                    .Configure(app =>
                               app.UseHealthChecks("/health", new HealthCheckOptions()
                               {
                                   Predicate = r => r.Tags.Contains("elasticsearch")
                               }));

                using (var server = new TestServer(webHostBuilder))
                {
                    await action(server);
                }

                await elastichsearchHost.StopAsync();
            }
        }
    }
}
