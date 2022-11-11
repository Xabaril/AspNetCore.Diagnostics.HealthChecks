using System.Collections;
using HealthChecks.IbmMQ;

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

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("ibmmq");
            check.ShouldBeOfType<IbmMQHealthCheck>();
        }
    }
}
