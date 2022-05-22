using FluentAssertions;
using HealthChecks.Solr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.SolR.Tests.DependencyInjection
{
    public class solr_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddSolr(options => { });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("solr");
            check.Should().BeOfType<SolrHealthCheck>();
        }
    }
}
