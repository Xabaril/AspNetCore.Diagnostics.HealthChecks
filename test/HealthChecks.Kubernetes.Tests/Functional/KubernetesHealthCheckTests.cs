using System.Net;
using k8s;

namespace HealthChecks.Kubernetes.Tests.Functional;

public class kubernetes_healthcheck_should
{
    [Fact]
    public async Task be_unhealthy_if_kubernetes_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddKubernetes(setup => setup.WithConfiguration(new KubernetesClientConfiguration
                    {
                        Host = "https://localhost:443",
                        SkipTlsVerify = true
                    }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer"), tags: new string[] { "k8s" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("k8s")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_empty_registrations()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddKubernetes(setup => setup.WithConfiguration(new KubernetesClientConfiguration
                    {
                        Host = "https://localhost:443",
                        SkipTlsVerify = true
                    }), tags: new string[] { "k8s" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("k8s")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(Skip = "TODO: Implement")]
    public async Task be_healthy_if_kubernetes_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddKubernetes(setup => setup.WithConfiguration(new KubernetesClientConfiguration
                    {
                        Host = "https://localhost:443",
                        SkipTlsVerify = true
                    }).CheckService("DummyService", s => s.Spec.Type == "LoadBalancer"), tags: new string[] { "k8s" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("k8s")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
