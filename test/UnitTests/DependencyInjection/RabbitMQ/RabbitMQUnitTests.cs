using System;
using System.Collections.Generic;
using FluentAssertions;
using HealthChecks.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.RabbitMQ
{
    public class rabbitmq_registration_should
    {
        private string _fakeConnectionString = "amqp://server";
        private string _defaultCheckName = "rabbitmq";

        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRabbitMQ(rabbitMQConnectionString: _fakeConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(_defaultCheckName);
            check.GetType().Should().Be(typeof(RabbitMQHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            var customCheckName = "my-"+ _defaultCheckName;

            services.AddHealthChecks()
                .AddRabbitMQ(_fakeConnectionString, name: customCheckName);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(customCheckName);
            check.GetType().Should().Be(typeof(RabbitMQHealthCheck));
        }

        [Fact]
        public async Task health_check_should_work_with_just_a_connection_passed_in()
        {
            var check = new RabbitMQHealthCheck(new MockConnection());

            var result = await check.CheckHealthAsync(new HealthCheckContext());

            result.Status.Should().Be(HealthStatus.Healthy);
        }

        [Fact]
        public async Task health_check_should_work_with_just_a_connection_factory_passed_in()
        {
            var check = new RabbitMQHealthCheck(new MockConnectionFactory());

            var result = await check.CheckHealthAsync(new HealthCheckContext());

            result.Status.Should().Be(HealthStatus.Healthy);
        }

        private class MockConnectionFactory : IConnectionFactory
        {
            public AuthMechanismFactory AuthMechanismFactory(IList<string> mechanismNames)
            {
                throw new NotImplementedException();
            }

            public IConnection CreateConnection()
            {
                throw new NotImplementedException();
            }

            public IConnection CreateConnection(string clientProvidedName)
            {
                return new MockConnection();
            }

            public IConnection CreateConnection(IList<string> hostnames)
            {
                throw new NotImplementedException();
            }

            public IConnection CreateConnection(IList<string> hostnames, string clientProvidedName)
            {
                throw new NotImplementedException();
            }

            public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints)
            {
                throw new NotImplementedException();
            }

            public IDictionary<string, object> ClientProperties { get; set; }
            public string Password { get; set; }
            public ushort RequestedChannelMax { get; set; }
            public uint RequestedFrameMax { get; set; }
            public ushort RequestedHeartbeat { get; set; }
            public bool UseBackgroundThreadsForIO { get; set; }
            public string UserName { get; set; }
            public string VirtualHost { get; set; }
            public Uri Uri { get; set; }
            public TaskScheduler TaskScheduler { get; set; }
            public TimeSpan HandshakeContinuationTimeout { get; set; }
            public TimeSpan ContinuationTimeout { get; set; }
        }

        private class MockConnection : IConnection
        {
            public int LocalPort { get; }
            public int RemotePort { get; }
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Abort()
            {
                throw new NotImplementedException();
            }

            public void Abort(ushort reasonCode, string reasonText)
            {
                throw new NotImplementedException();
            }

            public void Abort(int timeout)
            {
                throw new NotImplementedException();
            }

            public void Abort(ushort reasonCode, string reasonText, int timeout)
            {
                throw new NotImplementedException();
            }

            public void Close()
            {
                throw new NotImplementedException();
            }

            public void Close(ushort reasonCode, string reasonText)
            {
                throw new NotImplementedException();
            }

            public void Close(int timeout)
            {
                throw new NotImplementedException();
            }

            public void Close(ushort reasonCode, string reasonText, int timeout)
            {
                throw new NotImplementedException();
            }

            public IModel CreateModel()
            {
                return null;
            }

            public void HandleConnectionBlocked(string reason)
            {
                throw new NotImplementedException();
            }

            public void HandleConnectionUnblocked()
            {
                throw new NotImplementedException();
            }

            public bool AutoClose { get; set; }
            public ushort ChannelMax { get; }
            public IDictionary<string, object> ClientProperties { get; }
            public ShutdownEventArgs CloseReason { get; }
            public AmqpTcpEndpoint Endpoint { get; }
            public uint FrameMax { get; }
            public ushort Heartbeat { get; }
            public bool IsOpen { get; }
            public AmqpTcpEndpoint[] KnownHosts { get; }
            public IProtocol Protocol { get; }
            public IDictionary<string, object> ServerProperties { get; }
            public IList<ShutdownReportEntry> ShutdownReport { get; }
            public string ClientProvidedName { get; }
            public ConsumerWorkService ConsumerWorkService { get; }
            public event EventHandler<CallbackExceptionEventArgs> CallbackException;
            public event EventHandler<EventArgs> RecoverySucceeded;
            public event EventHandler<ConnectionRecoveryErrorEventArgs> ConnectionRecoveryError;
            public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;
            public event EventHandler<ShutdownEventArgs> ConnectionShutdown;
            public event EventHandler<EventArgs> ConnectionUnblocked;
        }
    }
}
