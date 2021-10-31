using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;
using HealthChecks.Dapr;
using Dapr.Client;

namespace UnitTests.HealthChecks.DependencyInjection.Dapr
{
    public class dapr_registration_should
    {
        private string _defaultCheckName = "dapr";

        [Fact]
        public void add_health_check_when_properly_configured_using_di()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new DaprClientBuilder().Build());
            services.AddHealthChecks()
                .AddDapr();

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(_defaultCheckName);
            check.GetType().Should().Be(typeof(DaprHealthCheck));
        }

        [Fact]
        public void add_health_check_when_properly_configured_using_arguments()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDapr(daprClient: new DaprClientBuilder().Build());

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(_defaultCheckName);
            check.GetType().Should().Be(typeof(DaprHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            var customCheckName = "my-" + _defaultCheckName;

            services.AddSingleton(new DaprClientBuilder().Build());
            services.AddHealthChecks()
                .AddDapr(name: customCheckName);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(customCheckName);
            check.GetType().Should().Be(typeof(DaprHealthCheck));
        }
    }
}
