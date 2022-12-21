using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using static HealthChecks.Nats.Tests.Defines;

namespace HealthChecks.Nats.Tests.Functional
{
    public class nats_healthcheck_should
    {
        [Fact]
        public Task be_healthy_if_nats_is_available_locally() =>
            FactAsync(
                setup => setup.Url = DefaultLocalConnectionString,
                async response => response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync()));

        [Fact]
        public Task be_healthy_for_official_demo_instance() =>
            FactAsync(
                setup => setup.Url = DemoConnectionString,
                async response => response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync()));

        [Fact]
        public Task be_healthy_if_nats_is_available_and_has_custom_name() =>
            FactAsync(
                setup => setup.Url = DefaultLocalConnectionString,
                async response => response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync()),
                name: "Demo");

        [Fact]
        public Task be_unhealthy_if_nats_endpoint_does_not_exist_or_is_offline() =>
            FactAsync(
                setup => setup.Url = ConnectionStringDoesNotExistOrStopped,
                async response =>
                {
                    response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
                    var content = await response.Content.ReadAsStringAsync();
                    var report = JsonSerializer.Deserialize<UIHealthReport>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } })!;
                    report.Status.ShouldBe(UIHealthStatus.Unhealthy);
                    report.Entries["nats"].Exception.ShouldBe("Failed to connect");

                });

        [Fact]
        public Task be_unhealthy_if_nats_endpoint_is_bogus() =>
            FactAsync(
                setup => setup.Url = "bogus",
                async response =>
                {
                    response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
                    var content = await response.Content.ReadAsStringAsync();
                    var report = JsonSerializer.Deserialize<UIHealthReport>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } })!;
                    report.Status.ShouldBe(UIHealthStatus.Unhealthy);
                    report.Entries["nats"].Exception.ShouldBe("Failed to connect");
                });

        [Fact]
        public Task be_unhealthy_if_nats_is_available_but_credentials_path_doesnt_exist() =>
            FactAsync(
                setup =>
                {
                    setup.Url = DefaultLocalConnectionString;
                    setup.CredentialsPath = CredentialsPathDoesnExist;
                },
                response => response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable),
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
                response => response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable),
                name: CredentialsPathDoesnExist);

        private async Task FactAsync(Action<NatsOptions> setupAction, Action<HttpResponseMessage> assertAction, string? name = null)
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

        private async Task FactAsync(Action<NatsOptions> setupAction, Func<HttpResponseMessage, Task> asyncAssertAction, string? name = null)
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
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                AllowCachingResponses = false
            });
    }
}
