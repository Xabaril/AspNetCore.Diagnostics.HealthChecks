using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly ILogger<AzureServiceBusQueueHealthCheck> _logger;

        public AzureServiceBusQueueHealthCheck(string connectionString, string queueName, ILogger<AzureServiceBusQueueHealthCheck> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureServiceBusQueueHealthCheck)} is checking the Azure Event Hub.");

                var queueClient = new QueueClient(_connectionString,
                    _queueName,
                    ReceiveMode.PeekLock);

                var scheduledMessageId = await queueClient.ScheduleMessageAsync(
                    new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE)),
                    new DateTimeOffset(DateTime.UtcNow).AddHours(2));

                await queueClient.CancelScheduledMessageAsync(scheduledMessageId);

                _logger?.LogInformation($"The {nameof(AzureServiceBusQueueHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureServiceBusQueueHealthCheck)} check fail for {_connectionString} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception: ex);
            }
        }
    }
}
