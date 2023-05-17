using System.Net;
using HealthChecks.UI.Client;

namespace HealthChecks.Uris.Tests.Functional
{
    public class uris_healthcheck_should
    {
        // httpbin.org is unstable
        private async Task Retry(TestServer server, int times = 6)
        {
            for (int i = 0; i < times; ++i)
            {
                using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
                if (i == times - 1)
                    response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                if (response.StatusCode == HttpStatusCode.OK)
                    break;
                await Task.Delay(1000 + Random.Shared.Next(100)).ConfigureAwait(false);
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
            await Retry(server).ConfigureAwait(false);
        }

        [Fact]
        public async Task be_healthy_if_method_is_available()
        {
            var uri = new Uri("https://httpbin.org/post");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddUrlGroup(uri, HttpMethod.Post, tags: new string[] { "uris" });
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
            await Retry(server).ConfigureAwait(false);
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
            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);
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
            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);
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
                    .AddUrlGroup(opt => opt.AddUri(uri, setup => setup.UseTimeout(TimeSpan.FromSeconds(1))), tags: new string[] { "uris" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("uris")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);
            response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_if_request_is_timeout_using_default_timeout()
        {
            var uri = new Uri($"https://httpbin.org/delay/3");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddUrlGroup(setup =>
                    {
                        setup.UseTimeout(TimeSpan.FromSeconds(1));
                        setup.AddUri(uri);
                    }, tags: new string[] { "uris" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("uris")
                    });
                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);
            response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_if_request_success_and_default_timeout_is_configured()
        {
            var uri = new Uri($"https://httpbin.org/delay/2");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddUrlGroup(setup =>
                    {
                        setup.UseTimeout(TimeSpan.FromSeconds(4));
                        setup.AddUri(uri);
                    }, tags: new string[] { "uris" });
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
            await Retry(server).ConfigureAwait(false);
        }

        [Fact]
        public async Task be_healthy_if_request_success_and_timeout_is_configured()
        {
            var uri = new Uri($"https://httpbin.org/delay/2");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddUrlGroup(opt => opt.AddUri(uri, setup => setup.UseTimeout(TimeSpan.FromSeconds(4))), tags: new string[] { "uris" });
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
            await Retry(server).ConfigureAwait(false);
        }

        [Fact]
        public async Task be_healthy_if_request_succeeds_and_expected_response_matches()
        {
            var uri = new Uri("https://httpbin.org/robots.txt");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddUrlGroup(opt =>
                        {
                            opt.AddUri(uri);
                        }, tags: new string[] { "uris" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("uris")
                    });
                });

            var server = new TestServer(webHostBuilder);
            await Retry(server).ConfigureAwait(false);
        }

        [Fact]
        public async Task be_unhealthy_if_request_succeeds_and_expected_response_fails()
        {
            var uri = new Uri("https://httpbin.org/robots.txt");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddUrlGroup(opt =>
                        {
                            opt.AddUri(uri, options =>
                            {
                                options.ExpectContent("non-existent");
                            });
                        }, tags: new string[] { "uris" });
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
}
