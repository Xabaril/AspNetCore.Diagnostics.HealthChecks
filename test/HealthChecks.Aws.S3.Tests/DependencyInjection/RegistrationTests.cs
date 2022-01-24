using Amazon.S3;
using FluentAssertions;
using HealthChecks.Aws.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace HealthChecks.Aws.S3.Tests.DependencyInjection
{
    public class aws_s3_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();


            //var awsOptions = Configuration.GetAWSOptions();
            //services.AddDefaultAWSOptions(awsOptions);

            //services.AddAWSService<IAmazonS3>();


            services.AddSingleton<IAmazonS3,AmazonS3Client>();
            services.AddHealthChecks()
                .AddS3(options =>
                {
                    options.BucketName = "bucket-name";
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

            services.AddSingleton<IAmazonS3, AmazonS3Client>();

            services.AddHealthChecks()
                 .AddS3(options =>
                 {
                    options.BucketName = "bucket-name";
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
