using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.RavenDB
{
    [Collection("execution")]
    public class secure_ravendb_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;
        private const string ConnectionString = "https://a.securesandbox.ravendb.community:8080";
        private readonly X509Certificate2 _cert;

        public secure_ravendb_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _cert = new X509Certificate2("./HealthChecks.RavenDB/cluster.server.certificate.securesandbox.pfx");
        }

        [Fact]
        public async Task be_healthy_if_ravendb_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecks()
                    .AddRavenDB(ConnectionString, tags: new string[] { "ravendb" }, clientCertificate:_cert);
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
        public async Task be_healthy_if_ravendb_is_available_and_contains_specific_database()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecks()
                    .AddRavenDB(ConnectionString, "Demo", tags: new string[] { "ravendb" }, clientCertificate:_cert);
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
                    .AddRavenDB(connectionString, tags: new string[] { "ravendb" });
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
                    .AddRavenDB(ConnectionString, "ThisDatabaseReallyDoesnExist", tags: new string[] { "ravendb" }, clientCertificate:_cert);
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
