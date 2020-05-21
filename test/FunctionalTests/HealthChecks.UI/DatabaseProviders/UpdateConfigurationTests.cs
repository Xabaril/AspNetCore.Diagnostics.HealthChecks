using FluentAssertions;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.UI.DatabaseProviders
{
    public class UpdateConfigurationTests
    {
        [Fact]
        public async Task update_healthchecks_uris_when_configuration_exists()
        {
            var endpointName = "endpoint1";
            var endpointUri = "http://server/sample";
            var updatedEndpointUri = $"{endpointUri}2";

            Func<string, ManualResetEventSlim, IWebHostBuilder> getHost = (uri, hostReset) =>
                new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .AddRouting()
                    .AddHealthChecksUI(setup =>
                    {
                        setup.AddHealthCheckEndpoint(endpointName, uri);
                    })
                    .AddSqliteStorage("Data Source = sqlite-updates.db");
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(setup =>
                    {
                        setup.MapHealthChecksUI();
                    });

                    var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                    lifetime.ApplicationStarted.Register(() =>
                    {
                        hostReset.Set();
                    });
                });

            var hostReset = new ManualResetEventSlim(false);
            using var host1 = new TestServer(getHost(endpointUri, hostReset));
            hostReset.Wait();

            var context = host1.Services.GetRequiredService<HealthChecksDb>();
            var configurations = await context.Configurations.ToListAsync();

            configurations[0].Name.Should().Be(endpointName);
            configurations[0].Uri.Should().Be(endpointUri);

            hostReset = new ManualResetEventSlim(false);
            using var host2 = new TestServer(getHost(updatedEndpointUri, hostReset));
            hostReset.Wait();

            context = host2.Services.GetRequiredService<HealthChecksDb>();
            configurations = await context.Configurations.ToListAsync();

            configurations[0].Name.Should().Be(endpointName);
            configurations[0].Uri.Should().Be(updatedEndpointUri);
        }
    }
}
