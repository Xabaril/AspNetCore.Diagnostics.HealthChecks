using System.Net;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
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
            _output.WriteLine($"Processors available to should_return_too_many_requests_status_code_when_exceding_configured_max_active_requests: {Environment.ProcessorCount}");

            int maxActiveRequests = 2;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddRouting()
                        .AddHealthChecks()
                        .AddAsyncCheck("Delayed", async () =>
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                            return HealthCheckResult.Healthy();
                        })
                        .Services
                        .AddHealthChecksUI(setup =>
                        {
                            setup.AddHealthCheckEndpoint("endpoint1", "http://localhost/health");
                            setup.SetApiMaxActiveRequests(maxActiveRequests);
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

            List<HttpResponseMessage> responses = new();

            // see discussion from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/1896
            var threads = Enumerable.Range(1, maxActiveRequests + 2)
                .Select(_ => new Thread(_ =>
                {
                    var r = server.CreateRequest("/healthchecks-api").GetAsync().Result;
                    lock (responses)
                        responses.Add(r);
                }))
                .ToList();

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            _output.WriteLine($"Statuses: {string.Join(" ", responses.Select(r => r.StatusCode))}");

            responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(responses.Count - maxActiveRequests);
            responses.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(maxActiveRequests);
        }

        [Fact]
        public void should_return_too_many_requests_status_using_default_server_max_active_requests()
        {
            _output.WriteLine($"Processors available to should_return_too_many_requests_status_using_default_server_max_active_requests: {Environment.ProcessorCount}");

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddRouting()
                        .AddHealthChecks()
                        .AddAsyncCheck("Delayed", async () =>
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                            return HealthCheckResult.Healthy();
                        })
                        .Services
                        .AddHealthChecksUI(setup => setup.AddHealthCheckEndpoint("endpoint1", "http://localhost/health"))
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

            List<HttpResponseMessage> responses = new();

            var serverSettings = server.Services.GetRequiredService<IOptions<Settings>>().Value;

            // see discussion from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/1896
            var threads = Enumerable.Range(1, serverSettings.ApiMaxActiveRequests + 2)
                .Select(_ => new Thread(_ =>
                {
                    var r = server.CreateRequest("/healthchecks-api").GetAsync().Result;
                    lock (responses)
                        responses.Add(r);
                }))
                .ToList();

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            _output.WriteLine($"Statuses: {string.Join(" ", responses.Select(r => r.StatusCode))}");

            responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(responses.Count - serverSettings.ApiMaxActiveRequests);
            responses.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(serverSettings.ApiMaxActiveRequests);
        }
    }
}
