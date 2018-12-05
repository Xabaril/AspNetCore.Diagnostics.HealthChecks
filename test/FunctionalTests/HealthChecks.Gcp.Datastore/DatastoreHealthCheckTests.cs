using FluentAssertions;
using FunctionalTests.Base;
using Google.Cloud.Datastore.V1;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.Gcp.Datastore
{
    [Collection("execution")]
    public class datastore_healthcheck_should
    {
        private const string ProjectId = "demo";
        private const string Host = "localhost";
        private const int Port = 8000;
        private const int BadPort = 8001;
        private readonly ExecutionFixture _fixture;

        public datastore_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_datastore_is_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                        .AddScoped<DatastoreDb>(sp => Create(Port))
                        .AddHealthChecks()
                        .AddGoogleCloudDatastore(timeOut: 1);
                })
                .Configure(app => { app.UseHealthChecks("/health"); });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_if_datastore_is_unavailable()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                        .AddScoped<DatastoreDb>(sp => Create(BadPort))
                        .AddHealthChecks()
                        .AddGoogleCloudDatastore(timeOut: 1);
                })
                .Configure(app => { app.UseHealthChecks("/health"); });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        private DatastoreDb Create(int port)
        {
            return DatastoreDb.Create(
                ProjectId,
                string.Empty,
                new DatastoreClientImpl(
                    new Google.Cloud.Datastore.V1.Datastore.DatastoreClient(
                        new Channel(Host, port, ChannelCredentials.Insecure)),
                        new DatastoreSettings()));
        }
    }
}