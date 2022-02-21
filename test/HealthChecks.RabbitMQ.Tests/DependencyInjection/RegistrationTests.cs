using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.RabbitMQ.Tests.DependencyInjection
{
    public class rabbitmq_registration_should
    {
        private const string FAKE_CONNECTION_STRING = "amqp://server";
        private const string DEFAULT_CHECK_NAME = "rabbitmq";

        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRabbitMQ(rabbitConnectionString: FAKE_CONNECTION_STRING);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(DEFAULT_CHECK_NAME);
            check.GetType().Should().Be(typeof(RabbitMQHealthCheck));

            ((RabbitMQHealthCheck)check).Dispose();
            var result = check.CheckHealthAsync(new HealthCheckContext { Registration = new HealthCheckRegistration("", check, null, null) }).Result;
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Exception.GetType().Should().Be(typeof(ObjectDisposedException));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            var customCheckName = "my-" + DEFAULT_CHECK_NAME;

            services.AddHealthChecks()
                .AddRabbitMQ(FAKE_CONNECTION_STRING, name: customCheckName);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(customCheckName);
            check.GetType().Should().Be(typeof(RabbitMQHealthCheck));

            ((RabbitMQHealthCheck)check).Dispose();
            var result = check.CheckHealthAsync(new HealthCheckContext { Registration = new HealthCheckRegistration("", check, null, null) }).Result;
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Exception.GetType().Should().Be(typeof(ObjectDisposedException));
        }

        [Fact]
        public void add_named_health_check_with_connection_string_factory_by_iServiceProvider_registered()
        {
            var services = new ServiceCollection();
            var customCheckName = "my-" + _defaultCheckName;
            services.AddSingleton(new RabbitMqSetting(){
                ConnectionString = _fakeConnectionString
            });

            services.AddHealthChecks()
                .AddRabbitMQ((sp)=> sp.GetRequiredService<RabbitMqSetting>().ConnectionString, name: customCheckName);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(customCheckName);
            check.GetType().Should().Be(typeof(RabbitMQHealthCheck));
        }

        [Fact]
        public void add_named_health_check_with_uri_string_factory_by_iServiceProvider_registered()
        {
            var services = new ServiceCollection();
            var customCheckName = "my-" + _defaultCheckName;
            services.AddSingleton(new RabbitMqSetting(){
                ConnectionString = _fakeConnectionString
            });

            services.AddHealthChecks()
                .AddRabbitMQ((sp)=> new Uri(sp.GetRequiredService<RabbitMqSetting>().ConnectionString), name: customCheckName);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(customCheckName);
            check.GetType().Should().Be(typeof(RabbitMQHealthCheck));
        }
    }

    public class RabbitMqSetting{
        public string ConnectionString { get; set; }
    }
}
