using System;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FunctionalTests.HealthChecks.DynamoDb
{
    [Collection("execution")]
    public class dynamodb_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public dynamodb_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_listing_all_tables_if_dynamodb_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddDynamoDb(options =>
                        {
                            options.ServiceUrl = "http://localhost:8000";
                            options.AccessKey = "key"; 
                            options.SecretKey = "key";
                        }, tags: new string[] {"dynamodb"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("dynamodb")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }
        
        [SkipOnAppVeyor]
        public async Task be_unhealthy_listing_all_tables_if_dynamodb_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddDynamoDb(options =>
                        {
                            options.ServiceUrl = "http://nonexistingdomain:8000";
                            options.AccessKey = "key"; 
                            options.SecretKey = "key";
                        }, tags: new string[] {"dynamodb"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("dynamodb")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
        
        [SkipOnAppVeyor]
        public async Task look_for_credentials_from_credentials_provider_chain_when_not_explicitly_provided()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddDynamoDb(options =>
                        {
                            options.ServiceUrl = "http://localhost:8000";
                        }, tags: new string[] {"dynamodb"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("dynamodb")
                    });
                });

            // Access key and secret key are not provided to DynamoDb health check, so it should look up credentials for provider chain
            // Environment variable is one of the credentials provider that AWS SDK uses
            // Note: for this test to be reliable, the host machine should not have ~/.aws/credentials file since that file will take
            // precedence over environment variable
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "access-key-id");
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "secret-access-key");
            
            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }
    }
}