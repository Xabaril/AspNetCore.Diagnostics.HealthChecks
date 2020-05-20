using Microsoft.AspNetCore.Hosting;
using UnitTests.Base;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks.UI.Core.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace UnitTests.UI.DatabaseProviders
{
    public class mysql_storage_provider_should
    {
        private const string ProviderName = "Pomelo.EntityFrameworkCore.MySql";
        [Fact]
        public void register_healthchecksdb_context_with_migrations()
        {
            var customOptionsInvoked = false;

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                    .AddMySqlStorage("Host=localhost;User Id=root;Password=Password12!;Database=UI", options => customOptionsInvoked = true);
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
