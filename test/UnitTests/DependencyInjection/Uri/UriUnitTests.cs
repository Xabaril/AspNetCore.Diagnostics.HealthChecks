using FluentAssertions;
using HealthChecks.Uris;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.UriGroup
{
    public class uri_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddUrlGroup(new Uri("http://httpbin.org/status/200"));

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("uri-group");
            check.Should().BeOfType<UriHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddUrlGroup(new Uri("http://httpbin.org/status/200"), name: "my-uri-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-uri-group");
            check.Should().BeOfType<UriHealthCheck>();
        }

        [Fact]
        public void add_health_check_when_configured_through_service_provider()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddUrlGroup(sp => new Uri("http://httpbin.org/status/200"));

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            check.Should().BeOfType<UriHealthCheck>();
        }
    }
}
