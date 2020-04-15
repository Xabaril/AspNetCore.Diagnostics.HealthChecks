using FluentAssertions;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.Base;
using Xunit;

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
                })
                .UseStartup<DefaultStartup>();

            var services = hostBuilder.Build().Services;
            var context = services.GetService<HealthChecksDb>();

            context.Should().NotBeNull();            
            context.Database.ProviderName.Should().Be(ProviderName);
            customOptionsInvoked.Should().BeTrue();
        }
    }
}
