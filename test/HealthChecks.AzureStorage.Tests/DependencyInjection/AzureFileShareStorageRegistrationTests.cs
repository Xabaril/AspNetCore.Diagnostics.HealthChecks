namespace HealthChecks.AzureStorage.Tests.DependencyInjection
{
    public class azurefilesharestorage_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureFileShare("FileEndpoint=https://unit-test.file.core.windows.net");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurefileshare");
            check.GetType().Should().Be(typeof(AzureFileShareHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureFileShare("FileEndpoint=https://unit-test.file.core.windows.net", name: "my-azurefileshare-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-azurefileshare-group");
            check.GetType().Should().Be(typeof(AzureFileShareHealthCheck));
        }
    }
}
