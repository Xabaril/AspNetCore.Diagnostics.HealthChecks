using Amazon.S3;

namespace HealthChecks.Aws.S3.Tests.DependencyInjection
{
    public class aws_s3_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddS3(options =>
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    options.AccessKey = "access-key";
                    options.BucketName = "bucket-name";
                    options.SecretKey = "secret-key";
                    options.S3Config = new AmazonS3Config();
#pragma warning restore CS0618 // Type or member is obsolete
                });

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
                 .AddS3(options =>
                 {
#pragma warning disable CS0618 // Type or member is obsolete
                     options.AccessKey = "access-key";
                     options.BucketName = "bucket-name";
                     options.SecretKey = "secret-key";
                     options.S3Config = new AmazonS3Config();
#pragma warning restore CS0618 // Type or member is obsolete
                 }, name: "aws s3 check");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("aws s3 check");
            check.GetType().Should().Be(typeof(S3HealthCheck));
        }
    }
}
