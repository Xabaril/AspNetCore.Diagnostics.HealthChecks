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
        private const string TEST_SESSIONID = "TestSessionId";
        private static readonly ConcurrentDictionary<string, QueueClient> _queueClientConnections = new ConcurrentDictionary<string, QueueClient>();

        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly bool _requiresSession;
        private readonly Action<Message> _configuringMessage;

        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName, Action<Message> configuringMessage = null, bool requiresSession = false)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }
            _connectionString = connectionString;
            _queueName = queueName;
            _configuringMessage = configuringMessage;
            _requiresSession = requiresSession;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionKey = $"{_connectionString}_{_queueName}";
                if (!_queueClientConnections.TryGetValue(connectionKey, out var queueClient))
                {
                    queueClient = new QueueClient(_connectionString, _queueName, ReceiveMode.PeekLock, RetryPolicy.NoRetry);

                    if (!_queueClientConnections.TryAdd(connectionKey, queueClient))
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: "New QueueClient connection can't be added into dictionary.");
                    }
                }

                var message = new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE));
                if (_requiresSession)
                {
                    message.SessionId = TEST_SESSIONID;
                }
                _configuringMessage?.Invoke(message);

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
