using FluentAssertions;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using UnitTests.Base;
using Xunit;

namespace UnitTests.UI.DatabaseProviders
{
    public class sqlserver_storage_provider_should
    {
        private const string ProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
        [Fact]
        public void register_healthchecksdb_context_with_migrations()
        {
            var customOptionsInvoked = false;

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                    .AddSqlServerStorage("connectionString", opt => customOptionsInvoked = true);
                })
                .UseStartup<DefaultStartup>();

            var services = hostBuilder.Build().Services;
            var context = services.GetService<HealthChecksDb>();

            context.Should().NotBeNull();
            context.Database.GetMigrations().Count().Should().BeGreaterThan(0);
            context.Database.ProviderName.Should().Be(ProviderName);
            customOptionsInvoked.Should().BeTrue();
        }
    }
}
