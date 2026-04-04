using System.Net;
using k8s;

namespace HealthChecks.Kubernetes.Tests.Functional;

public class kubernetes_healthcheck_should
{
    [Fact]
    public async Task be_unhealthy_if_kubernetes_is_unavailable()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddKubernetes(setup => setup.WithConfiguration(new KubernetesClientConfiguration
                        {
                            Host = "https://localhost:443",
                            SkipTlsVerify = true
                        }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer"), tags: ["k8s"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("k8s")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_empty_registrations()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddKubernetes(setup => setup.WithConfiguration(new KubernetesClientConfiguration
                        {
                            Host = "https://localhost:443",
                            SkipTlsVerify = true
                        }), tags: ["k8s"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("k8s")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(Skip = "TODO: Implement")]
    public async Task be_healthy_if_kubernetes_is_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddKubernetes(setup => setup.WithConfiguration(new KubernetesClientConfiguration
                        {
                            Host = "https://localhost:443",
                            SkipTlsVerify = true
                        }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer"), tags: ["k8s"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("k8s")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
