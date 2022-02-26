using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit;

namespace HealthChecks.MongoDb.Tests.DependencyInjection
{
    public class mongodb_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured_connectionString()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddMongoDb("mongodb://connectionstring");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("mongodb");
            check.GetType().Should().Be(typeof(MongoDbHealthCheck));
        }
        [Fact]
        public void add_health_check_when_properly_configured_mongoClientSettings()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddMongoDb(MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring")));

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("mongodb");
            check.GetType().Should().Be(typeof(MongoDbHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured_connectionString()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddMongoDb("mongodb://connectionstring", name: "my-mongodb-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-mongodb-group");
            check.GetType().Should().Be(typeof(MongoDbHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured_mongoClientSettings()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddMongoDb(MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring")), name: "my-mongodb-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-mongodb-group");
            check.GetType().Should().Be(typeof(MongoDbHealthCheck));
        }
    }
}
