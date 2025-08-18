using System.Text;
using k8s;
using Testcontainers.K3s;

namespace HealthChecks.Kubernetes.Tests;

public class K3sContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "rancher/k3s";

    private const string Tag = "v1.26.2-k3s1";

    public K3sContainer? Container { get; private set; }

    public async Task<KubernetesClientConfiguration> GetKubeconfigAsync()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        string? kubeconfig = await Container.GetKubeconfigAsync();

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(kubeconfig));

        return await KubernetesClientConfiguration.BuildConfigFromConfigFileAsync(stream);
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<K3sContainer> CreateContainerAsync()
    {
        var container = new K3sBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }
}
