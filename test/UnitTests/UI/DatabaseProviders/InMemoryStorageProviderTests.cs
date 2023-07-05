using HealthChecks.UI.Data;

namespace UnitTests.UI.DatabaseProviders
{
    public class in_memory_storage_provider_should
    {
        private const string ProviderName = "Microsoft.EntityFrameworkCore.InMemory";
        [Fact]
        public void register_healthchecksdb_context()
        {
            var customOptionsInvoked = false;

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                    .AddInMemoryStorage(opt => customOptionsInvoked = true);
                });

            var services = hostBuilder.Build().Services;
            var context = services.GetRequiredService<HealthChecksDb>();

            context.ShouldNotBeNull();
            context.Database.ProviderName.ShouldBe(ProviderName);
            customOptionsInvoked.ShouldBeTrue();
        }
    }
}
