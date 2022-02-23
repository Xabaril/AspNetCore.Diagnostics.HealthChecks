using System.Linq;
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
                    options.AccessKey = "access-key";
                    options.BucketName = "bucket-name";
                    options.SecretKey = "secret-key";
                    options.S3Config = new AmazonS3Config();
                });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

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
                     options.AccessKey = "access-key";
                     options.BucketName = "bucket-name";
                     options.SecretKey = "secret-key";
                     options.S3Config = new AmazonS3Config();
                 }, name: "aws s3 check");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("aws s3 check");
            check.GetType().Should().Be(typeof(S3HealthCheck));
        }
    }
}
