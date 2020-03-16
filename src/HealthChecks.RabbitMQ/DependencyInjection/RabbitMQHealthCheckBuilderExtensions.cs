using HealthChecks.RabbitMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQHealthCheckBuilderExtensions
    {
        private static ConcurrentDictionary<Guid, Lazy<RabbitMQHealthCheck>> _healthChecks = new ConcurrentDictionary<Guid, Lazy<RabbitMQHealthCheck>>();

        const string NAME = "rabbitmq";

        /// <summary>
        /// Add a health check for RabbitMQ services using connection string (amqp uri).
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="rabbitMQConnectionString">The RabbitMQ connection string to be used.</param>
        /// <param name="sslOption">The RabbitMQ ssl options. Optional. If <c>null</c>, the ssl option will counted as disabled and not used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'rabbitmq' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        /// <remarks>A single connection to the RabbitMQ broker will be reused for health checks.</remarks>
        public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder builder, string rabbitMQConnectionString, SslOption sslOption = null, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                new InternalRabbitMQHealthCheck(rabbitMQConnectionString, sslOption),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for RabbitMQ services using <see cref="IConnection"/> from service provider 
        /// or <see cref="IConnectionFactory"/> from service provider if none is found. At least one must be configured.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'rabbitmq' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        /// <remarks>A single connection to the RabbitMQ broker will be reused for health checks.</remarks>
        public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder builder, string name = default, 
            HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var guid = Guid.NewGuid();

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp =>
                {
                    if (!_healthChecks.ContainsKey(guid))
                    {
                        _healthChecks.TryAdd(guid, new Lazy<RabbitMQHealthCheck>(() =>
                        {
                            var connection = sp.GetService<IConnection>();
                            var connectionFactory = sp.GetService<IConnectionFactory>();
                            if (connection != null)
                            {
                                return new InternalRabbitMQHealthCheck(connection);
                            }
                            else if (connectionFactory != null)
                            {
                                return new InternalRabbitMQHealthCheck(connectionFactory);
                            }
                            else
                            {
                                throw new ArgumentException(
                                    $"Either an IConnection or IConnectionFactory must be registered with the service provider");
                            }
                        }));
                    }

                    return _healthChecks[guid].Value;
                },
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for RabbitMQ services using <see cref="IConnection"/> factory function
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionFactory"> A factory function to provide the rabbitMQ connection </param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'rabbitmq' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        /// <remarks>A single connection to the RabbitMQ broker will be reused for health checks.</remarks>
        public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder builder, Func<IServiceProvider, IConnection> connectionFactory, 
            string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var guid = Guid.NewGuid();

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp =>
                {
                    if (!_healthChecks.ContainsKey(guid))
                    {
                        _healthChecks.TryAdd(guid,
                            new Lazy<RabbitMQHealthCheck>(() =>
                            {
                                return new InternalRabbitMQHealthCheck(connectionFactory(sp));
                            }));
                    }

                    return _healthChecks[guid].Value;
                },
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for RabbitMQ services using <see cref="IConnectionFactory"/> factory function
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionFactoryFactory"> A factory function to provide the rabbitMQ connection factory</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'rabbitmq' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        /// <remarks>ONLY if IConnectionFactory.UseBackgroundThreadsForIO is true, then a single connection to the RabbitMQ broker will be reused for health checks.</remarks>
        public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder builder, Func<IServiceProvider, IConnectionFactory> connectionFactoryFactory, 
            string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var guid = Guid.NewGuid();

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp =>
                {
                    if (!_healthChecks.ContainsKey(guid))
                    {
                        _healthChecks.TryAdd(guid,
                            new Lazy<RabbitMQHealthCheck>(() =>
                            {
                                return new InternalRabbitMQHealthCheck(connectionFactoryFactory(sp));
                            }));
                    }

                    return _healthChecks[guid].Value;
                },
                failureStatus,
                tags,
                timeout));
        }
    }
}
