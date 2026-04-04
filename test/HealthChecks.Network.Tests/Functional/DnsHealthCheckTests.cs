using System.Net;

namespace HealthChecks.Network.Tests.Functional;

public class dns_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_configured_hosts_dns_resolution_matches()
    {
        var targetHost = "www.microsoft.com";
        var targetHostIpAddresses = Dns.GetHostAddresses(targetHost).Select(h => h.ToString()).ToArray();

        var targetHost2 = "localhost";
        var targetHost2IpAddresses = Dns.GetHostAddresses(targetHost2).Select(h => h.ToString()).ToArray();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddDnsResolveHealthCheck(setup =>
                    {
                        setup.ResolveHost(targetHost).To(targetHostIpAddresses)
                        .ResolveHost(targetHost2).To(targetHost2IpAddresses);
                    }, tags: ["dns"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("dns")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_dns_resolution_does_not_match_configured_hosts()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddDnsResolveHealthCheck(setup => setup.ResolveHost("www.microsoft.com").To("8.8.8.8", "5.5.5.5"), tags: ["dns"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("dns")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
