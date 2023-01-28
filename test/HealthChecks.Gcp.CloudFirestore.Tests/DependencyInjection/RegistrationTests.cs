namespace HealthChecks.Gcp.CloudFirestore.Tests.DependencyInjection
{
    public class cloud_firestore_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCloudFirestore(setup => setup.RequiredCollections = new string[] { });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("cloud firestore");
            check.ShouldBeOfType<CloudFirestoreHealthCheck>();
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCloudFirestore(setup => setup.RequiredCollections = new string[] { }, name: "my-cloud-firestore-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-cloud-firestore-group");
            check.ShouldBeOfType<CloudFirestoreHealthCheck>();
        }
    }
}
