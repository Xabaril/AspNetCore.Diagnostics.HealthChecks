using System.Net;
using Confluent.Kafka;

namespace HealthChecks.Kafka.Tests.Functional
{
    public class kafka_healthcheck_should
    {
        [Fact]
        public async Task be_unhealthy_if_kafka_is_unavailable()
        {
            var configuration = new ProducerConfig()
            {
                BootstrapServers = "localhost:0000",
                MessageSendMaxRetries = 0,
                MessageTimeoutMs = 1500,
                RequestTimeoutMs = 1500,
                SocketTimeoutMs = 1500
            };

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddKafka(configuration, tags: new string[] { "kafka" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("kafka")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .ShouldBe(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_if_kafka_is_available()
        {
            var configuration = new ProducerConfig()
            {
                BootstrapServers = "localhost:9092",
                MessageSendMaxRetries = 0
            };

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddKafka(configuration, tags: new string[] { "kafka" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("kafka")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .ShouldBe(HttpStatusCode.OK);
        }
    }
}
