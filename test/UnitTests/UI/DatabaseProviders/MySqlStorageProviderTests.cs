using FluentAssertions;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using UnitTests.Base;
using Xunit;

namespace UnitTests.UI.DatabaseProviders
{
    public class mysql_storage_provider_should
    {
        private const string ProviderName = "Pomelo.EntityFrameworkCore.MySql";

        [Fact(Skip = "Ignored meanwhile pomelo is not update to 1.0")]
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
            //customOptionsInvoked.Should().BeTrue();
        }
    }
}
