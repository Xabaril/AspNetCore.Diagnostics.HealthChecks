using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.RabbitMQ
{
    [Collection("execution")]
    public class rabbitmq_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public rabbitmq_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_rabbitmq_is_available()
        {
            var connectionString = @"amqp://localhost:5672";

            var webHostBuilder = new WebHostBuilder()
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddRabbitMQ(rabbitConnectionString: connectionString, tags: new string[] { "rabbitmq" });
                //services.AddHealthChecks()
                // .AddRabbitMQ(connectionString, sslOption: new SslOption(serverName: "localhost", enabled: false), tags: new string[] { "rabbitmq" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }


        [SkipOnAppVeyor]
        public async Task be_unhealthy_if_rabbitmq_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddRabbitMQ("amqp://localhost:6672", sslOption: new SslOption(serverName: "localhost", enabled: false), tags: new string[] { "rabbitmq" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("rabbitmq")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_rabbitmq_is_available_using_iconnectionfactory()
        {
            var connectionString = @"amqp://localhost:5672";

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                AutomaticRecoveryEnabled = true,
                Ssl = new SslOption(serverName: "localhost", enabled: false)
            };

            var webHostBuilder = new WebHostBuilder()
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRabbitMQ(sp => factory, tags: new string[] { "rabbitmq" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_rabbitmq_is_available_using_iconnection()
        {
            var connectionString = @"amqp://localhost:5672";

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                AutomaticRecoveryEnabled = true,
                Ssl = new SslOption(serverName: "localhost", enabled: false)
            };

            var connection = factory.CreateConnection();

            var webHostBuilder = new WebHostBuilder()
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton<IConnection>(connection)
                    .AddHealthChecks()
                    .AddRabbitMQ(tags: new string[] { "rabbitmq" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_rabbitmq_is_available_and_specify_default_ssloption()
        {
            var connectionString = @"amqp://localhost:5672";

            var webHostBuilder = new WebHostBuilder()
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddRabbitMQ(connectionString, sslOption: new SslOption(serverName: "localhost", enabled: false), tags: new string[] { "rabbitmq" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("rabbitmq")
                });
            });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }
    }
}
