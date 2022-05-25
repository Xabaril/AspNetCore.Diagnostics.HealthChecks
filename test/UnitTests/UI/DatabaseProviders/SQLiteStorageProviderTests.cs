using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.UI.DatabaseProviders
{
    public class sqlite_storage_provider_should
    {
        private const string ProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
        [Fact]
        public void register_healthchecksdb_context_with_migrations()
        {
            var customOptionsInvoked = false;

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                    .AddSqliteStorage("connectionString", options => customOptionsInvoked = true);
                });

            var services = hostBuilder.Build().Services;
            var context = services.GetRequiredService<HealthChecksDb>();

            context.Should().NotBeNull();
            context.Database.GetMigrations().Count().Should().BeGreaterThan(0);
            context.Database.ProviderName.Should().Be(ProviderName);
            customOptionsInvoked.Should().BeTrue();
        }
    }
}
