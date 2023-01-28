using System.Net;
using HealthChecks.Uris;
using Microsoft.AspNetCore.Http;
using RichardSzalay.MockHttp;

namespace UnitTests.Uris
{
    public class uris_healthcheck_should
    {
        private const string RequestUri = "http://localhost/mock";
        private const string hcname = "uri-healthcheck";

        [Fact]
        public async Task use_configured_http_client_and_handler_with_default_overload()
        {
            var services = new ServiceCollection();

            Action<IServiceProvider, HttpClient> clientConfigurationCallback = (_, client) => client.DefaultRequestHeaders.Add("MockHeader", "value");

            Func<IServiceProvider, HttpMessageHandler> configureHttpClientHandler = _ => GetMockedStatusCodeHandler(StatusCodes.Status200OK);

            services
                .AddHealthChecks()
                .AddUrlGroup(new Uri(RequestUri), hcname, configureClient: clientConfigurationCallback, configureHttpMessageHandler: configureHttpClientHandler);

            using var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(sp);
            var result = await check.CheckHealthAsync(new HealthCheckContext { Registration = registration }).ConfigureAwait(false);

            result.Status.ShouldBe(HealthStatus.Healthy);
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(hcname);
            client.DefaultRequestHeaders.Any(s => s.Key == "MockHeader").ShouldBeTrue();
        }

        [Fact]
        public async Task use_configured_http_client_and_handler_when_configuring_method()
        {
            var headerName = "X-API-KEY";
            var services = new ServiceCollection();

            services.AddSingleton(new ApiKeyConfiguration { HeaderName = headerName });

            Action<IServiceProvider, HttpClient> clientConfigurationCallback = (sp, client) =>
            {
                var keyConfiguration = sp.GetRequiredService<ApiKeyConfiguration>();
                client.DefaultRequestHeaders.Add(keyConfiguration.HeaderName, keyConfiguration.HeaderValue);
            };

            Func<IServiceProvider, HttpMessageHandler> configureHttpClientHandler = _ => GetMockedStatusCodeHandler(StatusCodes.Status500InternalServerError);

            services
                .AddHealthChecks()
                .AddUrlGroup(new Uri(RequestUri), HttpMethod.Post, hcname, configureClient: clientConfigurationCallback, configureHttpMessageHandler: configureHttpClientHandler);

            using var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(sp);
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(hcname);
            var result = await check.CheckHealthAsync(new HealthCheckContext { Registration = registration }).ConfigureAwait(false);

            result.Status.ShouldBe(HealthStatus.Unhealthy);
            client.DefaultRequestHeaders.Any(s => s.Key == headerName).ShouldBeTrue();
        }

        [Fact]
        public async Task use_configured_http_client_and_handler_when_using_setup_method()
        {
            var services = new ServiceCollection();

            Action<IServiceProvider, HttpClient> clientConfigurationCallback = (_, client) => client.DefaultRequestHeaders.Add("MockHeader", "value");

            Func<IServiceProvider, HttpMessageHandler> configureHttpClientHandler = _ => GetMockedStatusCodeHandler(400);

            services
                .AddHealthChecks()
                .AddUrlGroup(uriOptions: uriOptions => uriOptions.AddUri(new Uri(RequestUri)), name: hcname, configureClient: clientConfigurationCallback, configureHttpMessageHandler: configureHttpClientHandler);

            using var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(sp);
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(hcname);
            var result = await check.CheckHealthAsync(new HealthCheckContext { Registration = registration }).ConfigureAwait(false);

            client.DefaultRequestHeaders.Any(s => s.Key == "MockHeader").ShouldBeTrue();
            result.Status.ShouldBe(HealthStatus.Unhealthy);
        }

        [Fact]
        public void create_healthcheck_with_no_configured_httpclient_or_handler()
        {
            var services = new ServiceCollection();

            services
                .AddHealthChecks()
                .AddUrlGroup(new Uri(RequestUri));

            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var hc = registration.Factory(sp);
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(hcname);

            client.DefaultRequestHeaders.ShouldBeEmpty();
            hc.ShouldBeOfType<UriHealthCheck>();
        }

        private HttpMessageHandler GetMockedStatusCodeHandler(int statusCode)
        {
            var handler = new MockHttpMessageHandler();
            handler.Expect(RequestUri).Respond((HttpStatusCode)statusCode, "text/plain", "ok");

            return handler;
        }
    }

    internal class ApiKeyConfiguration
    {
        public string HeaderName = "X-API-KEY";
        public string HeaderValue = "my-api-key";
    }
}
