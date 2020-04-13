using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.InMemory.Storage;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.UI.DatabaseProviders
{
    public class inmemory_storage_should
    {

        [Fact]
        public async Task seed_database_and_serve_stored_executions()
        {
            var reset = new ManualResetEventSlim(false);
            var collectorReset = new ManualResetEventSlim(false);

            var webHostBuilder = HostBuilderHelper.Create(
                reset,
                collectorReset,
                configureUI: setup => setup.AddInMemoryStorage());

            var host = new TestServer(webHostBuilder);

            reset.Wait(ProviderTestHelper.DefaultHostTimeout);

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            var configurations = await context.Configurations.ToListAsync();

            var host1 = ProviderTestHelper.Endpoints[0];

            configurations[0].Name.Should().Be(host1.Name);
            configurations[0].Uri.Should().Be(host1.Uri);

            collectorReset.Wait(ProviderTestHelper.DefaultCollectorTimeout);

            using var client = host.CreateClient();

            var report = await client.GetAsJson<List<HealthCheckExecution>>("/healthchecks-api");
            report.First().Name.Should().Be(ProviderTestHelper.Endpoints[0].Name);
        }
    }
}