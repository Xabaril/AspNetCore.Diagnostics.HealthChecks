using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
#pragma warning disable 618

namespace FunctionalTests.HealthChecks.RavenDB
{
    [Collection("execution")]
    public class ravendb_with_single_connection_string_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;
        private const string ConnectionString = "http://localhost:9030";

        public ravendb_with_single_connection_string_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));

            try
            {
                using (var store = new DocumentStore
                {
                    Urls = new string[] { ConnectionString },
                })
                {
                    store.Initialize();


                    store.Maintenance.Server.Send(
                        new CreateDatabaseOperation(new DatabaseRecord("Demo")));
                }

            }
            catch { }
        }
        [SkipOnAppVeyor]
        public async Task be_healthy_if_ravendb_is_available()
        {

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecks()
                    .AddRavenDB(setup => setup.Urls = new[] { ConnectionString }, tags: new string[] { "ravendb" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("ravendb")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_ravendb_is_available_and_contains_specific_database()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecks()
                    .AddRavenDB(setup => setup.Urls = new[] { ConnectionString }, "Demo", tags: new string[] { "ravendb" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("ravendb")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_ravendb_is_not_available()
        {
            var connectionString = "http://localhost:9999";

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecks()
                    .AddRavenDB(setup => setup.Urls = new[] { connectionString }, tags: new string[] { "ravendb" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("ravendb")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_if_ravendb_is_available_but_database_doesnot_exist()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecks()
                    .AddRavenDB(setup => setup.Urls = new[] { ConnectionString }, "ThisDatabaseReallyDoesnExist", tags: new string[] { "ravendb" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("ravendb")
                    });
                });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
