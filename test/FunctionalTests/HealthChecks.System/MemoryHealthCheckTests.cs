using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.System
{
    [Collection("execution")]
    public class memory_healthcheck_should
    {
        private readonly ExecutionFixture fixture;

        public memory_healthcheck_should(ExecutionFixture fixture)
        {
            this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
        public async Task be_healthy_when_private_memory_does_not_exceed_the_maximum_established()
        {
            var currentMemory = Process.GetCurrentProcess().PrivateMemorySize64;
            var maximumMemory = currentMemory + 104857600;

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddPrivateMemoryHealthCheck(maximumMemory, tags: new string[] {"privatememory"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("privatememory")
                    });
                });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
        public async Task be_unhealthy_when_private_memory_does_exceed_the_maximum_established()
        {
            var currentMemory = Process.GetCurrentProcess().PrivateMemorySize64;
            var maximumMemory = currentMemory - 104857600;

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddPrivateMemoryHealthCheck(maximumMemory, tags: new string[] {"privatememory"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("privatememory")
                    });
                });


            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
        public async Task be_healthy_when_workingset_does_not_exceed_the_maximum_established()
        {
            var currentMemory = Process.GetCurrentProcess().WorkingSet64;
            var maximumMemory = currentMemory + 104857600;

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddWorkingSetHealthCheck(maximumMemory, tags: new string[] {"workingset"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("workingset")
                    });
                });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
        public async Task be_unhealthy_when_workingset_does_exceed_the_maximum_established()
        {
            var currentMemory = Process.GetCurrentProcess().WorkingSet64;
            var maximumMemory = currentMemory - 104857600;

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddPrivateMemoryHealthCheck(maximumMemory, tags: new string[] {"privatememory"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("privatememory")
                    });
                });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
        public async Task be_healthy_when_virtual_memory_size_does_not_exceed_the_maximum_established()
        {
            var currentMemory = Process.GetCurrentProcess().VirtualMemorySize64;
            var maximumMemory = currentMemory + 104857600;

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddVirtualMemorySizeHealthCheck(maximumMemory, tags: new string[] {"virtualmemory"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("virtualmemory")
                    });
                });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
        public async Task be_unhealthy_when_virtual_memory_size_does_exceed_the_maximum_established()
        {
            var currentMemory = Process.GetCurrentProcess().VirtualMemorySize64;
            var maximumMemory = currentMemory - 104857600;

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddVirtualMemorySizeHealthCheck(maximumMemory, tags: new string[] {"virtualmemory"});
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("virtualmemory")
                    });
                });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}