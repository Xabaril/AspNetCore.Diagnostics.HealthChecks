using Amazon.S3;
using HealthChecks.Aws.S3;

namespace UnitTests.HealthChecks.DependencyInjection.S3
{
    public class s3_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddS3(_ => { _.S3Config = new AmazonS3Config(); });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("aws s3");
            check.GetType().Should().Be(typeof(S3HealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddS3(_ => { _.S3Config = new AmazonS3Config(); }, name: "my-s3-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-s3-group");
            check.GetType().Should().Be(typeof(S3HealthCheck));
        }
    }
}
