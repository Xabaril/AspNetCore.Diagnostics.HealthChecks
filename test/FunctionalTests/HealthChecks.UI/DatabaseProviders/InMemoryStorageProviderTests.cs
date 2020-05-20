using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI.Core.Data;
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
    [Collection("execution")]
    public class inmemory_storage_should
    {

        [Fact]
        public async Task seed_database_and_serve_stored_executions()
        {
            var hostReset = new ManualResetEventSlim(false);
            var collectorReset = new ManualResetEventSlim(false);

            var webHostBuilder = HostBuilderHelper.Create(
                   hostReset,
                   collectorReset,
                   configureUI: config => config.AddInMemoryStorage());

            using var host = new TestServer(webHostBuilder);

            hostReset.Wait(ProviderTestHelper.DefaultHostTimeout);

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            var configurations = await context.Configurations.ToListAsync();
            var host1 = ProviderTestHelper.Endpoints[0];

            configurations[0].Name.Should().Be(host1.Name);
            configurations[0].Uri.Should().Be(host1.Uri);

            using var client = host.CreateClient();

            collectorReset.Wait(ProviderTestHelper.DefaultCollectorTimeout);

            var report = await client.GetAsJson<List<HealthCheckExecution>>("/healthchecks-api");
            report.First().Name.Should().Be(host1.Name);
        }
    }
}