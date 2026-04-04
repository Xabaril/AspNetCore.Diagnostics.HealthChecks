using System.Net;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;

namespace HealthChecks.Network.Tests.Functional;

public class dns_resolve_host_count_should
{
    private const string hostName = "google.com";
    private const string hostName2 = "microsoft.com";

    [Fact]
    public async Task be_healthy_when_the_configured_number_of_resolved_addresses_is_within_the_threshold()
    {
        int addresses = (await Dns.GetHostAddressesAsync(hostName)).Length;
        int addresses2 = (await Dns.GetHostAddressesAsync(hostName2)).Length;

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                    .AddRouting()
                    .AddHealthChecks()
                    .AddDnsResolveHostCountHealthCheck(setup =>
                    {
                        setup.AddHost(hostName, addresses, addresses);
                        setup.AddHost(hostName2, addresses2, addresses2);
                    });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                            app.MapHealthChecks("/health", new HealthCheckOptions
                            {
                                Predicate = r => true
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task be_unhealthy_when_the_configured_number_of_resolved_is_out_of_range()
    {
        int addresses = (await Dns.GetHostAddressesAsync(hostName)).Length;
        int addresses2 = (await Dns.GetHostAddressesAsync(hostName2)).Length;

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                    .AddRouting()
                    .AddHealthChecks()
                    .AddDnsResolveHostCountHealthCheck(setup =>
                    {
                        setup.AddHost(hostName, addresses + 1, addresses - 1);
                        setup.AddHost(hostName2, addresses2 + 1, addresses2 - 1);
                    });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                            app.MapHealthChecks("/health", new HealthCheckOptions
                            {
                                Predicate = r => true,
                                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
        await app.StartAsync();

        var response = await app.GetTestClient().GetAsJson<UIHealthReport>("/health");
        response.ShouldNotBeNull();
    }
}
