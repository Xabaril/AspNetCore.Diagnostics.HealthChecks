using System.Net;
using Confluent.Kafka;

namespace HealthChecks.Kafka.Tests.Functional;

public class kafka_healthcheck_should(KafkaContainerFixture kafkaContainerFixture) : IClassFixture<KafkaContainerFixture>
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

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddKafka(configuration, tags: ["kafka"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("kafka")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_kafka_is_available()
    {
        string connectionString = kafkaContainerFixture.GetConnectionString();

        var configuration = new ProducerConfig()
        {
            BootstrapServers = connectionString,
            MessageSendMaxRetries = 0
        };

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddKafka(configuration, tags: ["kafka"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("kafka")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
