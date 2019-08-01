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
        private static readonly ConcurrentDictionary<string, TopicClient> _topicClient = new ConcurrentDictionary<string, TopicClient>();
        private readonly Func<AzureServiceBusTopicHealthCheck, string> _partitionKeySelector;

        public AzureServiceBusTopicHealthCheck(string connectionString, string topicName, Func<AzureServiceBusTopicHealthCheck, string> partitionKeySelector = null)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            _connectionString = connectionString;
            _topicName = topicName;
            _partitionKeySelector = partitionKeySelector;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionKey = $"{_connectionString}_{_topicName}";
                if (!_topicClient.TryGetValue(connectionKey, out var topicClient))
                {
                    topicClient = new TopicClient(_connectionString, _topicName, RetryPolicy.NoRetry);

                    if (!_topicClient.TryAdd(connectionKey, topicClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "New TopicClient can't be added into dictionary.");
                    }
                }
                
                var message = new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE));

                if (_partitionKeySelector != null)
                    message.PartitionKey = _partitionKeySelector(this);

                var scheduledMessageId = await topicClient.ScheduleMessageAsync(message,
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
