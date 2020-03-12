using FluentAssertions;
using HealthChecks.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.System
{
    public class process_registration_should
    {
        [Fact]
        public void throw_exception_when_no_predicate_is_configured()
        {
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                services.AddHealthChecks()
                    .AddProcessHealthCheck("dotnet", null);
            });

            ex.Message.Should().Be("Value cannot be null. (Parameter 'predicate')");
        }

        [Fact]
        public void throw_exception_when_no_process_name_is_configured()
        {
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                services.AddHealthChecks()
                    .AddProcessHealthCheck("", p => p.Any());
            });

            ex.Message.Should().Be("Value cannot be null. (Parameter 'processName')");
        }

        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();

            services.AddHealthChecks()
                .AddProcessHealthCheck("dotnet", p => p?.Any() ?? false);
            
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("process");
            check.GetType().Should().Be(typeof(ProcessHealthCheck));
        }
    }
}
