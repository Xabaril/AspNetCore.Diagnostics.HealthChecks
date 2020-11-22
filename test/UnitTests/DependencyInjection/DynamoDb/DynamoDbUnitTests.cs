using Amazon;
using FluentAssertions;
using HealthChecks.DynamoDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.DynamoDb
{
    public class dynamoDb_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDynamoDb(_ => { _.AccessKey = "key"; _.SecretKey = "key"; _.RegionEndpoint = RegionEndpoint.CNNorth1; });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("dynamodb");
            check.GetType().Should().Be(typeof(DynamoDbHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDynamoDb(_ => { _.AccessKey = "key"; _.SecretKey = "key"; _.RegionEndpoint = RegionEndpoint.CNNorth1; }, name: "my-dynamodb-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-dynamodb-group");
            check.GetType().Should().Be(typeof(DynamoDbHealthCheck));
        }
    }
}
