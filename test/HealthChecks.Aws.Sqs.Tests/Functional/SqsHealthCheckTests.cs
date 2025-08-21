using System.Net;
using Amazon.Runtime;

namespace HealthChecks.Aws.Sqs.Tests.Functional;

public class aws_sqs_healthcheck_should(LocalStackContainerFixture localStackFixture) : IClassFixture<LocalStackContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_aws_sqs_queue_is_available()
    {
        string connectionString = localStackFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSqs(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;

                            options.AddQueue("healthchecks");
                        },
                        tags: ["sqs"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sqs")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_aws_sqs_multiple_queues_are_available()
    {
        string connectionString = localStackFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSqs(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;

                            options.AddQueue("healthchecks");
                            options.AddQueue("healthchecks");
                        },
                        tags: ["sqs"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sqs")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_aws_sqs_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSqs(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = "invalid";

                            options.AddQueue("healthchecks");
                        },
                        tags: ["sqs"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sqs")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_aws_sqs_credentials_not_provided()
    {
        string connectionString = localStackFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSqs(
                        options =>
                        {
                            options.ServiceURL = connectionString;

                            options.AddQueue("healthchecks");
                        },
                        tags: ["sqs"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sqs")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_aws_sqs_queue_does_not_exist()
    {
        string connectionString = localStackFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSqs(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;

                            options.AddQueue("invalid");
                        },
                        tags: ["sqs"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sqs")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
