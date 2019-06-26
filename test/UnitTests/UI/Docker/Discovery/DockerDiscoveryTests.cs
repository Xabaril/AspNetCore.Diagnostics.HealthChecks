using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Core.Discovery;
using HealthChecks.UI.Core.Discovery.Docker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace FunctionalTests.HealthChecks.UI.Docker
{
    public class DockerDiscoveryTests
    {
        class DiscoveryService : IDockerDiscoveryService
        {
            private readonly DockerDiscoveredContainer[] _containers;

            public DiscoveryService(params DockerDiscoveredContainer[] containers)
            {
                _containers = containers;
            }

            public Task<IList<DockerDiscoveredContainer>> Discover(CancellationToken cancellationToken)
            {
                return Task.FromResult<IList<DockerDiscoveredContainer>>(_containers);
            }
        }

        class DelegatingRegistryService : IDiscoveryRegistryService
        {
            private readonly Action<(string name, Uri uri)> _action;

            public DelegatingRegistryService(Action<(string name, Uri uri)> action)
            {
                _action = action;
            }

            public Task RegisterService(string service, string name, Uri uri, CancellationToken cancellationToken = default)
            {
                _action((name, uri));
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task discover_docker_services_and_add_to_registry()
        {
            var registereds = new List<(string name, Uri uri)>();
            var cancellationTokenSource = new CancellationTokenSource();

            var host = new HostBuilder().ConfigureServices(services =>
                {
                    services.Configure<DockerDiscoverySettings>(settings =>
                    {
                        settings.HealthPath = "/healthcheck";
                    });
                    services.AddSingleton<IDockerDiscoveryService>(_ => new DiscoveryService(new DockerDiscoveredContainer
                    {
                        // Only ip (should get scheme+port+path)
                        Name = "test1",
                        IP = "1.2.3.4"
                    }, new DockerDiscoveredContainer
                    {
                        // Ip + path (should get scheme+port)
                        Name = "test2",
                        IP = "1.2.3.4",
                        Path = "/hc"
                    }, new DockerDiscoveredContainer
                    {
                        // Scheme + ip + (blank) path
                        Name = "test3",
                        IP = "1.2.3.4",
                        Scheme = "https",
                        Path = ""
                    }, new DockerDiscoveredContainer
                    {
                        // Scheme + ip + port
                        Name = "test4",
                        IP = "1.2.3.4",
                        Port = 81,
                        Scheme = "https"
                    }));
                    services.AddSingleton<IDiscoveryRegistryService>(x => new DelegatingRegistryService(tuple =>
                    {
                        registereds.Add(tuple);
                        cancellationTokenSource.CancelAfter(100);
                    }));
                    services.AddHostedService<DockerDiscoveryHostedService>();
                })
                .Build();

            // Run host
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                await host.RunAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }

            // Evaluate
            Assert.Collection(registereds, tuple =>
            {
                Assert.Equal("test1", tuple.name);
                Assert.Equal(new Uri("http://1.2.3.4:80/healthcheck"), tuple.uri);
            }, tuple =>
            {
                Assert.Equal("test2", tuple.name);
                Assert.Equal(new Uri("http://1.2.3.4:80/hc"), tuple.uri);
            }, tuple =>
            {
                Assert.Equal("test3", tuple.name);
                Assert.Equal(new Uri("https://1.2.3.4:443/"), tuple.uri);
            }, tuple =>
            {
                Assert.Equal("test4", tuple.name);
                Assert.Equal(new Uri("https://1.2.3.4:81/healthcheck"), tuple.uri);
            });
        }
    }
}