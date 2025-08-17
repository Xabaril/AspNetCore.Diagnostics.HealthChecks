using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.Solr.Tests;

public class SolrContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/solr";

    private const string Tag = "9.9.0-slim";

    private const int Port = 8983;

    public IContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        var endpoint = new UriBuilder(Uri.UriSchemeHttp, Container.Hostname, Container.GetMappedPublicPort(Port))
        {
            Path = "/solr"
        };

        return endpoint.ToString();
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<IContainer> CreateContainerAsync()
    {
        var waitStrategy = Wait
            .ForUnixContainer()
            .UntilHttpRequestIsSucceeded(x => x.ForPath("/solr/solrcore/admin/ping").ForPort(Port));

        string hostConfigSetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "solr", "configsets");

        const string configSetsPath = "/opt/solr/server/solr/configsets";

        var container = new ContainerBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithResourceMapping(
                new DirectoryInfo(Path.Combine(hostConfigSetsPath, "solrcore")),
                $"{configSetsPath}/solrcore")
            .WithResourceMapping(
                new DirectoryInfo(Path.Combine(hostConfigSetsPath, "solrcoredown")),
                $"{configSetsPath}/solrcoredown")
            .WithCommand(
                "sh",
                "-c",
                $"precreate-core solrcore {configSetsPath}/solrcore && precreate-core solrcoredown {configSetsPath}/solrcoredown && solr-foreground")
            .WithPortBinding(Port, true)
            .WithWaitStrategy(waitStrategy)
            .Build();

        await container.StartAsync();

        return container;
    }
}
