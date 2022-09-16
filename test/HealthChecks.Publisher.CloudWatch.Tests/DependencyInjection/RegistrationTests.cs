namespace HealthChecks.Publisher.CloudWatch.Tests.DependencyInjection
{
    public class cloud_watch_publisher_registration_should
    {
        [Fact]
        public void add_healthcheck_when_properly_configured_with_default_parameter()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddCloudWatchPublisher();

            using var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }

        [Fact]
        public void add_healthcheck_when_properly_configured_with_custom_service_check_name()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddCloudWatchPublisher("serviceCheckName");

            using var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }

        [Fact]
        public void add_healthcheck_when_properly_configured_with_default_service_check_name_and_aws_credentials()
        {
            var services = new ServiceCollection();
            services
            .AddHealthChecks()
                .AddCloudWatchPublisher("awsAccessKeyId", "awsSecretAccessKey", Amazon.RegionEndpoint.USEast1);

            using var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }

        [Fact]
        public void add_healthcheck_when_properly_configured_with_custom_service_check_name_and_aws_credentials()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddCloudWatchPublisher("serviceCheckName", "awsAccessKeyId", "awsSecretAccessKey", Amazon.RegionEndpoint.USEast1);

            using var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }
    }
}
