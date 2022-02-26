using System.Linq;
using FluentAssertions;
using HealthChecks.NpgSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.Npgsql.Tests.DependencyInjection
{
    public class npgsql_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddNpgSql("connectionstring");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("npgsql");
            check.GetType().Should().Be(typeof(NpgSqlHealthCheck));

        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddNpgSql("connectionstring", name: "my-npg-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-npg-1");
            check.GetType().Should().Be(typeof(NpgSqlHealthCheck));
        }

        [Fact]
        public void add_health_check_with_connection_string_factory_when_properly_configured()
        {
            var services = new ServiceCollection();
            var factoryCalled = false;
            services.AddHealthChecks()
                .AddNpgSql(_ =>
                {
                    factoryCalled = true;
                    return "connectionstring";
                }, name: "my-npg-1");

            using var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-npg-1");
            check.GetType().Should().Be(typeof(NpgSqlHealthCheck));
            factoryCalled.Should().BeTrue();
        }
    }
}
