using FluentAssertions;
using HealthChecks.IbmMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Linq;
using Xunit;

namespace HealthChecks.Ibmq.Tests.DependencyInjection
{
    public class ibmq_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddIbmMQ("queue", new Hashtable());

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("ibmmq");
            check.Should().BeOfType<IbmMQHealthCheck>();
        }
    }
}
