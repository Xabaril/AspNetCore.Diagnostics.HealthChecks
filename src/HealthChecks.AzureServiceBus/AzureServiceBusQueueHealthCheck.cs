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
        : AzureServiceBusHealthCheck, IHealthCheck
    {
        private const string TEST_MESSAGE = "HealthCheckTest";
        private const string TEST_SESSIONID = "TestSessionId";
        private static readonly ConcurrentDictionary<string, QueueClient> _queueClientConnections = new ConcurrentDictionary<string, QueueClient>();
        
        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName, Action<Message> configuringMessage = null, bool requiresSession = false)
            : base(connectionString, queueName, configuringMessage, requiresSession)
        {
        }

        public AzureServiceBusQueueHealthCheck(string connectionString, Action<Message> configuringMessage = null, bool requiresSession = false)
            : base(connectionString, configuringMessage, requiresSession)
        {
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionKey = $"{Endpoint}_{EntityPath}";
                if (!_queueClientConnections.TryGetValue(connectionKey, out var queueClient))
                {
                    queueClient = new QueueClient(ConnectionStringBuilder, ReceiveMode.PeekLock, RetryPolicy.NoRetry);

                    if (!_queueClientConnections.TryAdd(connectionKey, queueClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "New QueueClient connection can't be added into dictionary.");
                    }
                }

                var message = new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE));
                if (RequiresSession)
                {
                    message.SessionId = TEST_SESSIONID;
                }
                ConfiguringMessage?.Invoke(message);

                var scheduledMessageId = await queueClient.ScheduleMessageAsync(message,
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
