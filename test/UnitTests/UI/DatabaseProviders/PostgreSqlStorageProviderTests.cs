using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.UI.DatabaseProviders
{
    public class postgre_sql_storage_provider_should
    {
        private const string ProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";
        [Fact]
        public void register_healthchecksdb_context_with_migrations()
        {
            var customOptionsInvoked = false;

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                    .AddPostgreSqlStorage("connectionString", options => customOptionsInvoked = true);
                });

            var services = hostBuilder.Build().Services;
            var context = services.GetRequiredService<HealthChecksDb>();

            context.ShouldNotBeNull();
            context.Database.GetMigrations().Count().ShouldBeGreaterThan(0);
            context.Database.ProviderName.ShouldBe(ProviderName);
            customOptionsInvoked.ShouldBeTrue();
        }
    }
}
