using System.Net;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;

namespace HealthChecks.UI.Tests
{
    public class ui_api_request_limiting
    {
        [Fact]
        public async Task should_return_too_many_requests_status_code_when_exceding_configured_max_active_requests()
        {
            int maxActiveRequests = 2;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddRouting()
                        .AddHealthChecks()
                        .AddAsyncCheck("Delayed", async () =>
                        {
                            await Task.Delay(200).ConfigureAwait(false);
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

            var requests = Enumerable.Range(1, maxActiveRequests + 2)
                .Select(_ => server.CreateRequest("/healthchecks-api").GetAsync()).ToList();

            var results = await Task.WhenAll(requests).ConfigureAwait(false);

            results.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(requests.Count - maxActiveRequests);
            results.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(maxActiveRequests);
        }

        [Fact]
        public async Task should_return_too_many_requests_status_using_default_server_max_active_requests()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddRouting()
                        .AddHealthChecks()
                        .AddAsyncCheck("Delayed", async () =>
                        {
                            await Task.Delay(200).ConfigureAwait(false);
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

            var serverSettings = server.Services.GetRequiredService<IOptions<Settings>>().Value;

            var requests = Enumerable.Range(1, serverSettings.ApiMaxActiveRequests + 2)
                .Select(_ => server.CreateRequest("/healthchecks-api").GetAsync()).ToList();

            var results = await Task.WhenAll(requests).ConfigureAwait(false);

            results.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(requests.Count - serverSettings.ApiMaxActiveRequests);
            results.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(serverSettings.ApiMaxActiveRequests);
        }
    }
}
