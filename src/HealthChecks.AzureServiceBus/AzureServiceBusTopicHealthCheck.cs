using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusTopicHealthCheck
        : IHealthCheck
    {
        private const string TEST_MESSAGE = "HealthCheckTestMessage";
        private readonly string _connectionString;
        private readonly string _topicName;
        private static readonly ConcurrentDictionary<string, ServiceBusConnection> ServiceBusConnections = new ConcurrentDictionary<string, ServiceBusConnection>();

        public AzureServiceBusTopicHealthCheck(string connectionString, string topicName)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(topicName)) throw new ArgumentNullException(nameof(topicName));

            _connectionString = connectionString;
            _topicName = topicName;
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

                var topicClient = new TopicClient(serviceBusConnection, _topicName, RetryPolicy.NoRetry);

                var scheduledMessageId = await topicClient.ScheduleMessageAsync(
                    new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE)),
                    new DateTimeOffset(DateTime.UtcNow).AddHours(2));

                await topicClient.CancelScheduledMessageAsync(scheduledMessageId);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
