using System.Net;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;

namespace HealthChecks.UI.Tests
{
    public class ui_api_request_limiting
    {
        [Fact]
        public void should_return_too_many_requests_status_code_when_exceding_configured_max_active_requests()
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
                            setup.ConfigureUIApiEndpointResult = _ => Thread.Sleep(200);
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

            var responses = new HttpResponseMessage[maxActiveRequests + 5];

            // see discussion from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/1896
            var threads = Enumerable.Range(1, maxActiveRequests + 5)
                .Select(i => new Thread(_ => responses[i] = server.CreateRequest(new Configuration.Options().ApiPath).GetAsync().Result))
                .ToList();

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

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
                            setup.ConfigureUIApiEndpointResult = _ => Thread.Sleep(200);
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
            var responses = new HttpResponseMessage[serverSettings.ApiMaxActiveRequests + 5];

            // see discussion from https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/1896
            var threads = Enumerable.Range(1, serverSettings.ApiMaxActiveRequests + 5)
                .Select(i => new Thread(_ => responses[i] = server.CreateRequest(new Configuration.Options().ApiPath).GetAsync().Result))
                .ToList();

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().ShouldBe(responses.Length - serverSettings.ApiMaxActiveRequests);
            responses.Where(r => r.StatusCode == HttpStatusCode.OK).Count().ShouldBe(serverSettings.ApiMaxActiveRequests);
        }
    }
}
