using FluentAssertions.Execution;

namespace HealthChecks.SendGrid.Tests.DependencyInjection
{
    public class sendgrid_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSendGrid("wellformed_but_invalid_token");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            using (new AssertionScope())
            {
                registration.Name.Should().Be("sendgrid");
                check.GetType().Should().Be<SendGridHealthCheck>();
            }
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSendGrid("wellformed_but_invalid_token", "my-sendgrid-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            using (new AssertionScope())
            {
                registration.Name.Should().Be("my-sendgrid-group");
                check.GetType().Should().Be(typeof(SendGridHealthCheck));
            }
        }
    }
}
