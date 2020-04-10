using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.UI.DatabaseProviders
{
    [Collection("execution")]
    public class postgre_storage_should
    {
        private readonly ExecutionFixture _fixture;

        public postgre_storage_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }
        [Fact]
        public async Task create_the_database_and_seed_configuration()
        {
            var reset = new ManualResetEventSlim(false);

            var webHostBuilder = new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI(setup => {
                        foreach (var item in ProviderTestHelper.Endpoints)
                        {
                            setup.AddHealthCheckEndpoint(item.Name, item.Uri);
                        }
                    })
                    .AddPostgreSqlStorage(ProviderTestHelper.PostgresConnectionString(_fixture))
                    .Services
                    .AddRouting();

                }).Configure(app =>
                {
                    app
                    .UseRouting()
                    .UseEndpoints(endpoints => {
                        endpoints.MapHealthChecksUI();
                    });

                    var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                    lifetime.ApplicationStarted.Register(() =>
                    {
                        reset.Set();
                    });

                }).UseStartup<DefaultStartup>();

            var host = new TestServer(webHostBuilder);

            reset.Wait(ProviderTestHelper.DefaultHostTimeout);

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            var configurations = await context.Configurations.ToListAsync();

            var host1 = ProviderTestHelper.Endpoints[0];
            var host2 = ProviderTestHelper.Endpoints[1];

            configurations[0].Name.Should().Be(host1.Name);
            configurations[0].Uri.Should().Be(host1.Uri);
            configurations[1].Name.Should().Be(host2.Name);
            configurations[1].Uri.Should().Be(host2.Uri);
        }
    }
}
