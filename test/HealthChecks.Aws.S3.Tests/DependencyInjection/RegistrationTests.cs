using Amazon.Runtime;
using Amazon.S3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

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
                    options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                    options.BucketName = "bucket-name";
                    options.S3Config = new AmazonS3Config();
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
                     options.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                     options.BucketName = "bucket-name";
                     options.S3Config = new AmazonS3Config();
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
