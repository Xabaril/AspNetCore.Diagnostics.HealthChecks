//using Amazon;
//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using Microsoft.Extensions.Options;
//using Xunit;

//namespace HealthChecks.Aws.Sqs.Tests.DependencyInjection
//{
//    public class aws_sqs_registration_should
//    {
//        [Fact]
//        public void add_health_check_when_properly_configured()
//        {
//            var services = new ServiceCollection();
//            _ = services.AddHealthChecks()
//                 .AddSqs(options =>
//                 {
//                     options.AccessKey = "access-key";
//                     options.SecretKey = "secret-key";
//                     options.Queues = new string[] { "queue1", "queue2" };
//                     options.RegionEndpoint = RegionEndpoint.EUCentral1;
//                 }, name: "aws s3 check");

//            using var serviceProvider = services.BuildServiceProvider();
//            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

//            var registration = options.Value.Registrations.First();
//            var check = registration.Factory(serviceProvider);

//            registration.Name.Should().Be("aws sqs");
//            check.GetType().Should().Be(typeof(SqsHealthCheck));
//        }
//        [Fact]
//        public void add_named_health_check_when_properly_configured()
//        {
//            var services = new ServiceCollection();
//            _ = services.AddHealthChecks()
//                .AddSqs(options =>
//                {
//                    options.AccessKey = "access-key";
//                    options.SecretKey = "secret-key";
//                    options.Queues = new string[] { "queue1", "queue2" };
//                    options.RegionEndpoint = RegionEndpoint.EUCentral1;
//                }, name: "aws s3 check");

//            using var serviceProvider = services.BuildServiceProvider();
//            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

//            var registration = options.Value.Registrations.First();
//            var check = registration.Factory(serviceProvider);

//            registration.Name.Should().Be("aws s3 check");
//            check.GetType().Should().Be(typeof(SqsHealthCheck));
//        }
//    }
//}
