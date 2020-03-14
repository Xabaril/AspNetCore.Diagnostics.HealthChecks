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
    public class process_allocated_memory_healthcheck_should
    {
        [Fact]
        public void add_healthcheck_when_properly_configured()
        {
            var services = new ServiceCollection();

            services.AddHealthChecks()
                .AddProcessAllocatedMemoryHealthCheck(10);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("process_allocated_memory");
            check.GetType().Should().Be(typeof(ProcessAllocatedMemoryHealthCheck));
        }

        [Fact]
        public void fail_registration_when_configured_with_wrong_maximum_maximum_bytes_allocated()
        {
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                services.AddHealthChecks()
                    .AddProcessAllocatedMemoryHealthCheck(-15);
            });
        }
    }
}
