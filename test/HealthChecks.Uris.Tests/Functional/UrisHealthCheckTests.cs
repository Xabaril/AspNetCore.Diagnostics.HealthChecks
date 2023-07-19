using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.Uris.Tests.Functional;

public class uris_healthcheck_should
{
    private sealed class DelayStubMessageHandler : HttpClientHandler
    {
        private readonly TimeSpan _delay;

        public DelayStubMessageHandler(TimeSpan delay)
        {
            _delay = delay;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class ContentStubMessageHandler : HttpClientHandler
    {
        private readonly string _content;

        public ContentStubMessageHandler(string content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(_content) });
        }
    }

    [Fact]
    public async Task be_healthy_if_uri_is_available()
    {
        var uri = new Uri("https://httpbin.org/get");

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddUrlGroup(uri, tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);
        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_method_is_available()
    {
        var uri = new Uri("https://httpbin.org/post"); // does not matter

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddUrlGroup(
                        uri,
                        HttpMethod.Post,
                        configurePrimaryHttpMessageHandler: _ => new DelayStubMessageHandler(TimeSpan.Zero),
                        tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task be_unhealthy_if_uri_is_not_available()
    {
        var uri = new Uri("http://200.0.0.100");

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddUrlGroup(uri, tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris")
                });
            });

        using var server = new TestServer(webHostBuilder);
        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [InlineData(199)]
    [InlineData(300)]
    [Theory]
    public async Task be_unhealthy_if_status_code_is_error(int statusCode)
    {
        var uri = new Uri($"https://httpbin.org/status/{statusCode}");

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddUrlGroup(uri, tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris")
                });
            });

        using var server = new TestServer(webHostBuilder);
        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_request_is_timeout()
    {
        var uri = new Uri($"https://httpbin.org/delay/2");

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddUrlGroup(
                    opt => opt.AddUri(uri, options => options.UseTimeout(TimeSpan.FromSeconds(1))),
                    tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris")
                });
            });

        using var server = new TestServer(webHostBuilder);
        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_request_is_timeout_using_default_timeout()
    {
        var uri = new Uri($"https://httpbin.org/delay/11"); // does not matter

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddUrlGroup(
                    opt =>
                    {
                        // opt.UseTimeout(TimeSpan.FromSeconds(1)); use default timeout - 10 seconds
                        opt.AddUri(uri);
                    },
                    configurePrimaryHttpMessageHandler: _ => new DelayStubMessageHandler(TimeSpan.FromSeconds(11)),
                    tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris")
                });
            });

        using var server = new TestServer(webHostBuilder);
        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_request_success_and_timeout_is_configured()
    {
        var uri = new Uri($"https://httpbin.org/delay/2"); // does not matter

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddUrlGroup(
                        opt => opt.AddUri(uri, options => options.UseTimeout(TimeSpan.FromSeconds(3))),
                        configurePrimaryHttpMessageHandler: _ => new DelayStubMessageHandler(TimeSpan.FromSeconds(2)),
                        tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task be_healthy_if_request_success_and_default_timeout_is_configured()
    {
        var uri = new Uri($"https://httpbin.org/delay/2"); // does not matter

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddUrlGroup(
                        opt =>
                        {
                            // opt.UseTimeout(TimeSpan.FromSeconds(4)); use default timeout - 10 seconds
                            opt.AddUri(uri);
                        },
                        configurePrimaryHttpMessageHandler: _ => new DelayStubMessageHandler(TimeSpan.FromSeconds(2)),
                        tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task be_healthy_if_request_succeeds_and_expected_response_matches()
    {
        var uri = new Uri("https://httpbin.org/robots.txt"); // does not matter

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddUrlGroup(
                        opt => opt.AddUri(uri, options => options.ExpectContent("abc")),
                        configurePrimaryHttpMessageHandler: _ => new ContentStubMessageHandler("abc"),
                        tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris")
                });
            });

        var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task be_unhealthy_if_request_succeeds_and_expected_response_fails()
    {
        var uri = new Uri("https://httpbin.org/robots.txt"); // does not matter

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddUrlGroup(
                        opt => opt.AddUri(uri, options => options.ExpectContent("xyz")),
                        configurePrimaryHttpMessageHandler: _ => new ContentStubMessageHandler("abc"),
                        tags: new string[] { "uris" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("uris")
                });
            });

        var server = new TestServer(webHostBuilder);
        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
