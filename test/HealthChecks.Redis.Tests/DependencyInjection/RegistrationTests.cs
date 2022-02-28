using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.Redis.Tests.DependencyInjection
{
    public class redis_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRedis("connectionstring");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("redis");
            check.GetType().Should().Be(typeof(RedisHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRedis("connectionstring", name: "my-redis");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-redis");
            check.GetType().Should().Be(typeof(RedisHealthCheck));
        }

        [Fact]
        public void add_health_check_with_connection_string_factory_when_properly_configured()
        {
            var services = new ServiceCollection();
            var factoryCalled = false;
            services.AddHealthChecks()
                .AddRedis(_ =>
                {
                    factoryCalled = true;
                    return "connectionstring";
                });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("redis");
            check.GetType().Should().Be(typeof(RedisHealthCheck));
            factoryCalled.Should().BeTrue();
        }
    }
}
