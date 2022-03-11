using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.Oracle.Tests.DependencyInjection
{
    public class oracle_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddOracle("connectionstring");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("oracle");
            check.GetType().Should().Be(typeof(OracleHealthCheck));

        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddOracle("connectionstring", name: "my-oracle-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-oracle-1");
            check.GetType().Should().Be(typeof(OracleHealthCheck));
        }

        [Fact]
        public void add_health_check_with_connection_string_factory_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddOracle(_ => "connectionstring");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("oracle");
            check.GetType().Should().Be(typeof(OracleHealthCheck));
        }
    }
}
