using System.Net;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace HealthChecks.UI.Tests
{
    public class ui_api_request_limiting
    {
        private readonly ITestOutputHelper _output;

        public ui_api_request_limiting(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void should_return_too_many_requests_status_code_when_exceding_configured_max_active_requests()
        {
            int maxActiveRequests = 2;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .AddLogging(builder => builder.AddXUnit(_output))
                        .AddRouting()
                        .AddHealthChecks()
                        .Services
                        .AddHealthChecksUI(setup =>
                        {
                            setup.AddHealthCheckEndpoint("endpoint1", "http://localhost/health");
                            setup.SetApiMaxActiveRequests(maxActiveRequests);
                            setup.ConfigureUIApiEndpointResult = _ => Task.Delay(1000);
                        })
                        .AddInMemoryStorage(databaseName: "LimitingTests");
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(setup =>
                    {
                        setup.MapHealthChecks("/health", new HealthCheckOptions
                        {
                            Predicate = _ => true,
                            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                        });

                        setup.MapHealthChecksUI();
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var responses = new HttpResponseMessage[maxActiveRequests + 1];

            // see discussion from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/1896
            var threads = Enumerable.Range(0, maxActiveRequests + 1)
                .Select(i => new Thread(_ => responses[i] = server.CreateRequest(new Configuration.Options().ApiPath).GetAsync().Result))
                .ToList();

            threads.ForEach(t =>
            {
                t.Start();
                _output.WriteLine($"Thread {t.ManagedThreadId} started at {DateTime.Now:hh:mm:ss.FFFF}");
            });
            threads.ForEach(t =>
            {
                t.Join();
                _output.WriteLine($"Thread {t.ManagedThreadId} ended at {DateTime.Now:hh:mm:ss.FFFF}");
            });

            responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(responses.Length - maxActiveRequests);
            responses.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(maxActiveRequests);
        }

        [Fact]
        public void should_return_too_many_requests_status_using_default_server_max_active_requests()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddRouting()
                        .AddHealthChecks()
                        .Services
                        .AddHealthChecksUI(setup =>
                        {
                            setup.AddHealthCheckEndpoint("endpoint1", "http://localhost/health");
                            setup.ConfigureUIApiEndpointResult = _ => Task.Delay(1000);
                        })
                        .AddInMemoryStorage(databaseName: "LimitingTests");
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(setup =>
                    {
                        setup.MapHealthChecks("/health", new HealthCheckOptions
                        {
                            Predicate = _ => true,
                            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                        });

                        setup.MapHealthChecksUI();
                    });

                });

            using var server = new TestServer(webHostBuilder);

            var serverSettings = server.Services.GetRequiredService<IOptions<Settings>>().Value;
            var responses = new HttpResponseMessage[serverSettings.ApiMaxActiveRequests + 1];

            // see discussion from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/1896
            var threads = Enumerable.Range(0, serverSettings.ApiMaxActiveRequests + 1)
                .Select(i => new Thread(_ => responses[i] = server.CreateRequest(new Configuration.Options().ApiPath).GetAsync().Result))
                .ToList();

            threads.ForEach(t =>
            {
                t.Start();
                _output.WriteLine($"Thread {t.ManagedThreadId} started at {DateTime.Now:hh:mm:ss.FFFF}");
            });
            threads.ForEach(t =>
            {
                t.Join();
                _output.WriteLine($"Thread {t.ManagedThreadId} ended at {DateTime.Now:hh:mm:ss.FFFF}");
            });

            responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(responses.Length - serverSettings.ApiMaxActiveRequests);
            responses.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(serverSettings.ApiMaxActiveRequests);
        }
    }
}
