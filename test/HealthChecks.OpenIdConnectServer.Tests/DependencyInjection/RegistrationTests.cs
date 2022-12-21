namespace HealthChecks.IdSvr.Tests.DependencyInjection
{
    public class idsvr_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddIdentityServer(new Uri("http://myidsvr"));

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("idsvr");
            check.ShouldBeOfType<IdSvrHealthCheck>();
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddIdentityServer(new Uri("http://myidsvr"), name: "my-idsvr-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-idsvr-group");
            check.ShouldBeOfType<IdSvrHealthCheck>();
        }
        [Fact]
        public void add_health_check_when_properly_configured_with_uri_provider()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddIdentityServer(sp => new Uri("http://myidsvr"));

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("idsvr");
            check.ShouldBeOfType<IdSvrHealthCheck>();
        }
        [Fact]
        public void add_named_health_check_when_properly_configured_with_uri_provider()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddIdentityServer(sp => new Uri("http://myidsvr"), name: "my-idsvr-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-idsvr-group");
            check.ShouldBeOfType<IdSvrHealthCheck>();
        }
    }
}
