using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusQueueHealthCheck
        : IHealthCheck
    {
        private const string TEST_MESSAGE = "HealthCheckTest";
        private readonly string _connectionString;
        private readonly string _queueName;
        private static readonly ConcurrentDictionary<string, ServiceBusConnection> ServiceBusConnections = new ConcurrentDictionary<string, ServiceBusConnection>();

        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(queueName)) throw new ArgumentNullException(nameof(queueName));
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (ServiceBusConnections.TryGetValue(_connectionString, out var serviceBusConnection))
                {
                    serviceBusConnection = new ServiceBusConnection(_connectionString);

                    if (!ServiceBusConnections.TryAdd(_connectionString, serviceBusConnection))
                    {
                        return
                            new HealthCheckResult(context.Registration.FailureStatus, description: "New service bus connection can't be added into dictionary.");
                    }
                }

                var queueClient = new QueueClient(serviceBusConnection,
                    _queueName,
                    ReceiveMode.PeekLock,
                    RetryPolicy.NoRetry);

                var scheduledMessageId = await queueClient.ScheduleMessageAsync(
                    new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE)),
                    new DateTimeOffset(DateTime.UtcNow).AddHours(2));

                await queueClient.CancelScheduledMessageAsync(scheduledMessageId);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
