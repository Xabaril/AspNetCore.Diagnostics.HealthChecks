using System.Net;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
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
        public async Task should_return_too_many_requests_status_code_when_exceding_configured_max_active_requests()
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
                        .AddInMemoryStorage<DelayedHealthChecksDb>(databaseName: "LimitingTests");
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

            // warmup
            (await server.CreateRequest("/healthchecks-api").GetAsync().ConfigureAwait(false)).StatusCode.ShouldBe(HttpStatusCode.OK);

            var requests = Enumerable.Range(1, maxActiveRequests + 2)
                .Select(_ => server.CreateRequest(new Configuration.Options().ApiPath).GetAsync())
                .ToList();

            var results = await Task.WhenAll(requests).ConfigureAwait(false);

            _output.WriteLine($"Statuses: {string.Join(" ", requests.Select(r => r.Result.StatusCode))}");

            results.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(requests.Count - maxActiveRequests);
            results.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(maxActiveRequests);
        }

        [Fact]
        public async Task should_return_too_many_requests_status_using_default_server_max_active_requests()
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
                        .AddInMemoryStorage<DelayedHealthChecksDb>(databaseName: "LimitingTests");
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

            // warmup
            (await server.CreateRequest("/healthchecks-api").GetAsync().ConfigureAwait(false)).StatusCode.ShouldBe(HttpStatusCode.OK);

            var requests = Enumerable.Range(1, serverSettings.ApiMaxActiveRequests + 2)
                .Select(_ => server.CreateRequest(new Configuration.Options().ApiPath).GetAsync())
                .ToList();

            var results = await Task.WhenAll(requests).ConfigureAwait(false);

            _output.WriteLine($"Statuses: {string.Join(" ", requests.Select(r => r.Result.StatusCode))}");

            results.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(requests.Count - serverSettings.ApiMaxActiveRequests);
            results.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(serverSettings.ApiMaxActiveRequests);
        }

        private sealed class DelayedHealthChecksDb : HealthChecksDb
        {
            public DelayedHealthChecksDb(DbContextOptions<DelayedHealthChecksDb> options) : base(options)
            {
            }

            public override void Dispose()
            {
                Thread.Sleep(200);
                base.Dispose();
            }

            public override async ValueTask DisposeAsync()
            {
                await Task.Delay(200).ConfigureAwait(false);
                await base.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
