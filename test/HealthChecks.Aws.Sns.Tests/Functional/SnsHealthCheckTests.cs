using System.Net;
using Amazon.Runtime;
using HealthChecks.Aws.Sns.Tests;

namespace HealthChecks.Aws.Sqs.Tests.Functional;

public class aws_sqs_healthcheck_should(LocalStackContainerFixture localStackFixture) : IClassFixture<LocalStackContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_aws_sns_topic_is_available()
    {
        string connectionString = localStackFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSnsTopicsAndSubscriptions(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;

                            options.AddTopicAndSubscriptions("healthchecks");
                        },
                        tags: ["sns"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sns")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_aws_sns_multiple_queues_are_available()
    {
        string connectionString = localStackFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSnsTopicsAndSubscriptions(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;

                            options.AddTopicAndSubscriptions("healthchecks");
                            options.AddTopicAndSubscriptions("healthchecks");
                        },
                        tags: ["sns"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sns")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_aws_sns_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSnsTopicsAndSubscriptions(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = "invalid";

                            options.AddTopicAndSubscriptions("healthchecks");
                        },
                        tags: ["sns"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sns")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_aws_sns_credentials_not_provided()
    {
        string connectionString = localStackFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSnsTopicsAndSubscriptions(
                        options =>
                        {
                            options.ServiceURL = connectionString;

                            options.AddTopicAndSubscriptions("healthchecks");
                        },
                        tags: ["sns"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sns")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_aws_sns_topic_does_not_exist()
    {
        string connectionString = localStackFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddSnsTopicsAndSubscriptions(
                        options =>
                        {
                            options.Credentials = new BasicAWSCredentials("test", "test");
                            options.ServiceURL = connectionString;

                            options.AddTopicAndSubscriptions("invalid");
                        },
                        tags: ["sns"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sns")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
