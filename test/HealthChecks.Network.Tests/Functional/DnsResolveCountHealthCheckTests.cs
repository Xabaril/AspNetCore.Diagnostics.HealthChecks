using System.Net;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;

namespace HealthChecks.Network.Tests.Functional
{
    public class dns_resolve_host_count_should
    {
        private const string hostName = "google.com";
        private const string hostName2 = "microsoft.com";

        [Fact]
        public async Task be_healthy_when_the_configured_number_of_resolved_addresses_is_within_the_threshold()
        {
            int addresses = (await Dns.GetHostAddressesAsync(hostName)).Length;
            int addresses2 = (await Dns.GetHostAddressesAsync(hostName2)).Length;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .AddRouting()
                    .AddHealthChecks()
                    .AddDnsResolveHostCountHealthCheck(setup =>
                    {
                        setup.AddHost(hostName, addresses, addresses);
                        setup.AddHost(hostName2, addresses2, addresses2);
                    });
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(config =>
                    {
                        config.MapHealthChecks("/health", new HealthCheckOptions
                        {
                            Predicate = r => true
                        });
                    });

                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_unhealthy_when_the_configured_number_of_resolved_is_out_of_range()
        {
            int addresses = (await Dns.GetHostAddressesAsync(hostName)).Length;
            int addresses2 = (await Dns.GetHostAddressesAsync(hostName2)).Length;

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .AddRouting()
                    .AddHealthChecks()
                    .AddDnsResolveHostCountHealthCheck(setup =>
                    {
                        setup.AddHost(hostName, addresses + 1, addresses - 1);
                        setup.AddHost(hostName2, addresses2 + 1, addresses2 - 1);
                    });
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(config =>
                    {
                        config.MapHealthChecks("/health", new HealthCheckOptions
                        {
                            Predicate = r => true,
                            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                        });
                    });

                });

            using var server = new TestServer(webHostBuilder);
            var response = await server.CreateClient().GetAsJson<UIHealthReport>("/health");
            response.ShouldNotBeNull();
        }
    }
}
