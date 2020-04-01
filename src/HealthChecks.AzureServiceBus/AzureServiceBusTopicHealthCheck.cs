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
        : AzureServiceBusHealthCheck, IHealthCheck
    {
        private const string TEST_MESSAGE = "HealthCheckTestMessage";
        private const string TEST_SESSIONID = "TestSessionId";
        private static readonly ConcurrentDictionary<string, TopicClient> _topicClient = new ConcurrentDictionary<string, TopicClient>();

        public AzureServiceBusTopicHealthCheck(string connectionString, string topicName, Action<Message> configuringMessage = null, bool requiresSession = false)
        : base(connectionString, topicName, configuringMessage, requiresSession)
        {
        }

        public AzureServiceBusTopicHealthCheck(string connectionString, Action<Message> configuringMessage = null, bool requiresSession = false)
            : base(connectionString, configuringMessage, requiresSession)
        {
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionKey = $"{Endpoint}_{EntityPath}";
                if (!_topicClient.TryGetValue(connectionKey, out var topicClient))
                {
                    topicClient = new TopicClient(ConnectionStringBuilder, RetryPolicy.NoRetry);

                    if (!_topicClient.TryAdd(connectionKey, topicClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "New TopicClient can't be added into dictionary.");
                    }
                }

                var message = new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE));
                if (RequiresSession)
                {
                    message.SessionId = TEST_SESSIONID;
                }

                ConfiguringMessage?.Invoke(message);

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
