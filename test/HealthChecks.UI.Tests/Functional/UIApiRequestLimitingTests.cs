using System.Net;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.Extensions.Logging;

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
                        .Services
                        .AddHealthChecksUI(setup =>
                        {
                            setup.AddHealthCheckEndpoint("endpoint1", "http://localhost/health");
                            setup.SetApiMaxActiveRequests(maxActiveRequests);
                            setup.ConfigureUIApiEndpointResult = _ => Task.Delay(200);
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

            var tasks = Enumerable.Range(0, maxActiveRequests + 5)
                .Select(_ => server.CreateRequest(new Configuration.Options().ApiPath).GetAsync())
                .ToList();

            var responses = await Task.WhenAll(tasks).ConfigureAwait(false);

            responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(responses.Length - maxActiveRequests);
            responses.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(maxActiveRequests);
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
                        .Services
                        .AddHealthChecksUI(setup =>
                        {
                            setup.AddHealthCheckEndpoint("endpoint1", "http://localhost/health");
                            setup.ConfigureUIApiEndpointResult = _ => Task.Delay(200);
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

            var tasks = Enumerable.Range(0, serverSettings.ApiMaxActiveRequests + 5)
                .Select(_ => server.CreateRequest(new Configuration.Options().ApiPath).GetAsync())
                .ToList();

            var responses = await Task.WhenAll(tasks).ConfigureAwait(false);

            responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(responses.Length - serverSettings.ApiMaxActiveRequests);
            responses.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(serverSettings.ApiMaxActiveRequests);
        }
    }
}
