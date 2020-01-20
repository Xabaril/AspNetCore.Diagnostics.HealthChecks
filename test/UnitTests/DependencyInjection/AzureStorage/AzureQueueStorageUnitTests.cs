using FluentAssertions;
using HealthChecks.AzureStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureStorage
{
    public class azurequeuestorage_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureQueueStorage("the-connection-string");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureQueueStorageHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureQueueStorage("the-connection-string", name: "my-azurequeue-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-azurequeue-group");
            check.GetType().Should().Be(typeof(AzureQueueStorageHealthCheck));
        }

        [Fact]
        public void add_custom_tagged_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureQueueStorage("the-connection-string", name: "my-azurequeue-group", tags: new[] { "custom-tag" });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-azurequeue-group");
            registration.Tags.Should().Contain("custom-tag");
            check.GetType().Should().Be(typeof(AzureQueueStorageHealthCheck));
        }

        [Fact]
        public void add_health_check_with_custom_failure_status_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureQueueStorage("the-connection-string", name: "my-azurequeue-group", failureStatus: HealthStatus.Degraded);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-azurequeue-group");
            registration.FailureStatus.Should().Be(HealthStatus.Degraded);
            check.GetType().Should().Be(typeof(AzureQueueStorageHealthCheck));
        }

        [Fact]
        public void add_named_queue_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureQueueStorage("the-connection-string", queueName: "queue");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureQueueStorageHealthCheck));
        }
    }
}
