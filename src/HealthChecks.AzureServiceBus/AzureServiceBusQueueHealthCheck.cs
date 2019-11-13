using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
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

        /// <summary>
        /// Null value for this field specifies not being able to connect to queue yet to know session requirement.
        /// This could happen if queue is not yet created or gets deleted, after health check starts.
        /// </summary>
        private bool? _requiresSession;
        private readonly Action<Message> _configuringMessage;

        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName, Action<Message> configuringMessage = null)
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

                // If the queue is not yet available, GetQueueAsync will throw and _requiresSession will not have any value
                if (!_requiresSession.HasValue)
                {
                    var managementClient = new ManagementClient(_connectionString);
                    var queueDescription = await managementClient.GetQueueAsync(_queueName);
                    _requiresSession = queueDescription.RequiresSession;
                }

                var message = new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE));
                if (_requiresSession.Value)
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
                // Reset _requiresSession to handle cases of queue not available once the health check probe is up
                // If queue becomes available at a later time, we should again figure out whether sessions are needed.
                _requiresSession = null;
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
