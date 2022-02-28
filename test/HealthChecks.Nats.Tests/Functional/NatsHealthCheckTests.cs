using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static HealthChecks.Nats.Tests.Defines;

namespace HealthChecks.Nats.Tests.Functional
{
    public class nats_healthcheck_should
    {
        [Fact]
        public Task be_healthy_if_nats_is_available_locally() =>
            FactAsync(
                setup => setup.Url = DefaultLocalConnectionString,
                response => response.StatusCode.Should().Be(HttpStatusCode.OK));

        [Fact]
        public Task be_healthy_for_official_demo_instance() =>
            FactAsync(
                setup => setup.Url = DemoConnectionString,
                response => response.StatusCode.Should().Be(HttpStatusCode.OK));

        [Fact]
        public Task be_healthy_if_nats_is_available_and_has_custom_name() =>
            FactAsync(
                setup => setup.Url = DefaultLocalConnectionString,
                response => response.StatusCode.Should().Be(HttpStatusCode.OK),
                name: "Demo");

        [Fact]
        public Task be_unhealthy_if_nats_endpoint_does_not_exist_or_is_offline() =>
            FactAsync(
                setup => setup.Url = ConnectionStringDoesNotExistOrStopped,
                async response =>
                {
                    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
                    var content = await response.Content.ReadAsStringAsync();
                    content.Should().Be("Unhealthy");
                });

        [Fact]
        public Task be_unhealthy_if_nats_endpoint_is_bogus() =>
            FactAsync(
                setup => setup.Url = "bogus",
                async response =>
                {
                    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
                    var content = await response.Content.ReadAsStringAsync();
                    content.Should().Be("Unhealthy");
                });

        [Fact]
        public Task be_unhealthy_if_nats_is_available_but_credentials_path_doesnt_exist() =>
            FactAsync(
                setup =>
                {
                    setup.Url = DefaultLocalConnectionString;
                    setup.CredentialsPath = CredentialsPathDoesnExist;
                },
                response => response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable),
                name: CredentialsPathDoesnExist);

        [Fact]
        public Task be_unhealthy_if_nats_20_credentials_missing() =>
            FactAsync(
                setup =>
                {
                    setup.Url = DefaultLocalConnectionString;
                    setup.CredentialsPath = string.Empty;
                    setup.Jwt = "jwt";
                    setup.PrivateNKey = CredentialsPathDoesnExist;
                },
                response => response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable),
                name: CredentialsPathDoesnExist);

        private async Task FactAsync(Action<NatsOptions> setupAction, Action<HttpResponseMessage> assertAction, string name = null)
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services => services
                        .AddHealthChecks()
                        .AddNats(
                            setupAction,
                            name: name,
                            tags: Tags))
                .Configure(ConfigureApplicationBuilder);

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest(HealthRequestRelativePath).GetAsync();

            assertAction(response);
        }

        private async Task FactAsync(Action<NatsOptions> setupAction, Func<HttpResponseMessage, Task> asyncAssertAction, string name = null)
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services => services
                        .AddHealthChecks()
                        .AddNats(
                            setupAction,
                            name: name,
                            tags: Tags))
                .Configure(ConfigureApplicationBuilder);

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest(HealthRequestRelativePath).GetAsync();

            await asyncAssertAction(response);
        }

        private void ConfigureApplicationBuilder(IApplicationBuilder app) =>
            app.UseHealthChecks(HealthRequestRelativePath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains(NatsName) || r.Name == NatsName,
                AllowCachingResponses = false
            });
    }
}
