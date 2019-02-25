using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.RabbitMQ
{
    public class RabbitMQHealthCheck
        : IHealthCheck
    {
        private readonly string _rabbitMqConnectionString;
        private readonly SslOption _sslOption;

        public RabbitMQHealthCheck(string rabbitMqConnectionString, SslOption sslOption = null)
        {
            _rabbitMqConnectionString = rabbitMqConnectionString ?? throw new ArgumentNullException(nameof(rabbitMqConnectionString));
            _sslOption = sslOption ?? new SslOption("not_enabled", enabled: false);
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(_rabbitMqConnectionString)
                };

                using (var connection = CreateConnection(factory, _sslOption))
                using (var channel = connection.CreateModel())
                {
                    return Task.FromResult(
                        HealthCheckResult.Healthy());
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
        private static IConnection CreateConnection(IConnectionFactory connectionFactory, SslOption sslOption)
        {
            return connectionFactory.CreateConnection(new List<AmqpTcpEndpoint>{new AmqpTcpEndpoint(connectionFactory.Uri, sslOption)});
        }
    }
}
