namespace HealthChecks.System.Tests.DependencyInjection
{
    public class system_registration_should
    {
        [Fact]
        public void throw_exception_when_no_predicate_is_configured()
        {
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                services.AddHealthChecks()
                    .AddProcessHealthCheck("dotnet", null!);
            });

            ex.Message.ShouldBe("Value cannot be null. (Parameter 'predicate')");
        }

        [Fact]
        public void throw_exception_when_no_process_name_is_configured()
        {
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => services.AddHealthChecks().AddProcessHealthCheck("", p => p.Length > 0));

            ex.Message.ShouldBe("Value cannot be null. (Parameter 'processName')");
        }

        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();

            services.AddHealthChecks()
                .AddProcessHealthCheck("dotnet", p => p?.Any() ?? false);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("process");
            check.ShouldBeOfType<ProcessHealthCheck>();
        }
    }
}
