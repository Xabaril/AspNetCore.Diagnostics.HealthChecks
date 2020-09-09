using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.UI
{

    public class ui_api_request_limiting
    {
        [Fact]
        public async Task should_return_too_many_requests_status_code_when_exceding_configured_max_active_requests()
        {
            int maxActiveRequests = 2;

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                   .ConfigureServices(services =>
                   {
                       services
                            .AddRouting()
                           .AddHealthChecks()
                           .AddAsyncCheck("Delayed", async () =>
                           {
                               await Task.Delay(200);
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
                               Predicate = r => true,
                               ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                           });

                           setup.MapHealthChecksUI();
                       });

                   });

            var server = new TestServer(webHostBuilder);

            var requests = Enumerable.Range(1, maxActiveRequests)
                .Select(n => server.CreateRequest($"/healthchecks-api").GetAsync());

            var results = await Task.WhenAll(requests);

            results.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests).Count().Should().Be(requests.Count() - maxActiveRequests);
            results.Where(r => r.StatusCode == HttpStatusCode.OK).Count().Should().Be(maxActiveRequests);

        }

        [Fact]
        public async Task should_return_too_many_requests_status_using_default_server_max_active_requests()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                   .ConfigureServices(services =>
                   {
                       services
                            .AddRouting()
                           .AddHealthChecks()
                           .AddAsyncCheck("Delayed", async () =>
                           {
                               await Task.Delay(200);
                               return HealthCheckResult.Healthy();
                           })
                           .Services
                           .AddHealthChecksUI(setup =>
                           {
                               setup.AddHealthCheckEndpoint("endpoint1", "http://localhost/health");
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
                               Predicate = r => true,
                               ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                           });

                           setup.MapHealthChecksUI();
                       });

                   });

            var server = new TestServer(webHostBuilder);

            var serverSettings = server.Services.GetRequiredService<IOptions<Settings>>().Value;

            var requests = Enumerable.Range(1, serverSettings.ApiMaxActiveRequests)
                .Select(n => server.CreateRequest($"/healthchecks-api").GetAsync());

            var results = await Task.WhenAll(requests);

            results.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                .Count()
                .Should().Be(requests.Count() - serverSettings.ApiMaxActiveRequests);

            results.Where(r => r.StatusCode == HttpStatusCode.OK).Count()
                .Should().Be(serverSettings.ApiMaxActiveRequests);

        }
    }
}
