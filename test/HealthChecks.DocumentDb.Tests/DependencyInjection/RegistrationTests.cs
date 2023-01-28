namespace HealthChecks.DocumentDb.Tests.DependencyInjection
{
    public class documentdb_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDocumentDb(_ => { _.PrimaryKey = "key"; _.UriEndpoint = "endpoint"; });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("documentdb");
            check.ShouldBeOfType<DocumentDbHealthCheck>();
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDocumentDb(_ => { _.PrimaryKey = "key"; _.UriEndpoint = "endpoint"; }, name: "my-documentdb-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-documentdb-group");
            check.ShouldBeOfType<DocumentDbHealthCheck>();
        }
    }
}
