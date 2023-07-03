using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.UI.DatabaseProviders
{
    public class mysql_storage_provider_should
    {
        private const string PROVIDER_NAME = "Pomelo.EntityFrameworkCore.MySql";

        [Fact(Skip = "Ignored meanwhile pomelo is not update to 1.0")]
        public void register_healthchecksdb_context_with_migrations()
        {
            var customOptionsInvoked = false;

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                    .AddMySqlStorage("Host=localhost;User Id=root;Password=Password12!;Database=UI", options => customOptionsInvoked = true);
                });

            var services = hostBuilder.Build().Services;
            var context = services.GetRequiredService<HealthChecksDb>();

            context.ShouldNotBeNull();
            context.Database.GetMigrations().Count().ShouldBeGreaterThan(0);
            context.Database.ProviderName.ShouldBe(PROVIDER_NAME);
            customOptionsInvoked.ShouldBeTrue();
        }
    }
}
