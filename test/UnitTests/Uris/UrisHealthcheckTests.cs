using FluentAssertions;
using HealthChecks.Uris;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

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

            Action<IServiceProvider, HttpClient> clientConfigurationCallback = (_ , client) => client.DefaultRequestHeaders.Add("MockHeader", "value");

            Func<IServiceProvider, HttpMessageHandler> configureHttpClientHandler = _ => GetMockedStatusCodeHandler(StatusCodes.Status200OK);

            services
                .AddHealthChecks()
                .AddUrlGroup(new Uri(RequestUri), hcname, configureClient: clientConfigurationCallback, configureHttpMessageHandler: configureHttpClientHandler);

            var sp = services.BuildServiceProvider();
            var options = sp.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(sp);
            var result = await check.CheckHealthAsync(new HealthCheckContext { Registration = registration });

            result.Status.Should().Be(HealthStatus.Healthy);
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(hcname);
            client.DefaultRequestHeaders.Any(s => s.Key == "MockHeader").Should().BeTrue();
            
        }

        [Fact]
        public async Task use_configured_http_client_and_handler_when_configuring_method()
        {
            var headerName = "X-API-KEY";
            var services = new ServiceCollection();

            services.AddSingleton(new ApiKeyConfiguration { HeaderName = headerName });

            Action<IServiceProvider, HttpClient> clientConfigurationCallback = (sp, client) => {
                var keyConfiguration = sp.GetRequiredService<ApiKeyConfiguration>();
                client.DefaultRequestHeaders.Add(keyConfiguration.HeaderName, keyConfiguration.HeaderValue);
            };

            Func<IServiceProvider, HttpMessageHandler> configureHttpClientHandler = _ => GetMockedStatusCodeHandler(StatusCodes.Status500InternalServerError);

            services
                .AddHealthChecks()
                .AddUrlGroup(new Uri(RequestUri), HttpMethod.Post, hcname, configureClient: clientConfigurationCallback, configureHttpMessageHandler: configureHttpClientHandler);

            var sp = services.BuildServiceProvider();
            var options = sp.GetService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(sp);
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(hcname);
            var result = await check.CheckHealthAsync(new HealthCheckContext { Registration = registration });

            result.Status.Should().Be(HealthStatus.Unhealthy);
            client.DefaultRequestHeaders.Any(s => s.Key == headerName).Should().BeTrue();
        }

        [Fact]
        public async Task use_configured_http_client_and_handler_when_using_setup_method()
        {
            var services = new ServiceCollection();

            Action<IServiceProvider, HttpClient> clientConfigurationCallback = (_, client) => client.DefaultRequestHeaders.Add("MockHeader", "value");

            Func<IServiceProvider, HttpMessageHandler> configureHttpClientHandler = _ => GetMockedStatusCodeHandler(400);

            services
                .AddHealthChecks()
                .AddUrlGroup(uriOptions: uriOptions => uriOptions.AddUri(new Uri(RequestUri)) , name: hcname, configureClient: clientConfigurationCallback, configureHttpMessageHandler: configureHttpClientHandler);

            var sp = services.BuildServiceProvider();
            var options = sp.GetService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(sp);
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(hcname);
            var result = await check.CheckHealthAsync(new HealthCheckContext { Registration = registration });

            client.DefaultRequestHeaders.Any(s => s.Key == "MockHeader").Should().BeTrue();
            result.Status.Should().Be(HealthStatus.Unhealthy);
        }


        [Fact]
        public void create_healthcheck_with_no_configured_httpclient_or_handler()
        {
            var services = new ServiceCollection();

            services
                .AddHealthChecks()
                .AddUrlGroup(new Uri(RequestUri));

            var sp = services.BuildServiceProvider();
            var options = sp.GetService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var hc = registration.Factory(sp);
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(hcname);

            client.DefaultRequestHeaders.Should().BeEmpty();
            hc.Should().BeOfType<UriHealthCheck>();

        }

        private HttpMessageHandler GetMockedStatusCodeHandler(int statusCode)
        {
            var handler = new MockHttpMessageHandler();
            handler.Expect(RequestUri).Respond((HttpStatusCode) statusCode, "text/plain", "ok");

            return handler;
        }
    }
    internal class ApiKeyConfiguration
    {
        public string HeaderName = "X-API-KEY";
        public string HeaderValue = "my-api-key";
    }
}
