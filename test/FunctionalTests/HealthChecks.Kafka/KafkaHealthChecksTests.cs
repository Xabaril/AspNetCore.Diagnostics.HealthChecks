using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.Kafka
{
    [Collection("execution")]
    public class kafka_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public kafka_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_if_kafka_is_unavailable()
        {
            var config = new Dictionary<string, object>()
            {
                { "bootstrap.servers", "localhost:0000"},
                { "message.send.max.retries", 0 },
                { "default.topic.config", new Dictionary<string, object>()
                    {
                        { "message.timeout.ms", 5000 }
                    }
                }
            };

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddKafka(config, tags: new string[] { "kafka" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("kafka")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_kafka_is_available()
        {
            var config = new Dictionary<string, object>()
            {
                { "bootstrap.servers", "localhost:9092"}
            };

            var webHostBuilder = new WebHostBuilder()
                 .UseStartup<DefaultStartup>()
                 .ConfigureServices(services =>
                 {
                     services.AddHealthChecks()
                     .AddKafka(config, tags: new string[] { "kafka" });
                 })
                 .Configure(app =>
                 {
                     app.UseHealthChecks("/health", new HealthCheckOptions
                     {
                         Predicate = r => r.Tags.Contains("kafka")
                     });
                 });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }
    }
}
