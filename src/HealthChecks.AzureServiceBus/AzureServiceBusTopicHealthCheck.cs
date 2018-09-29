using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly ILogger<AzureServiceBusTopicHealthCheck> _logger;

        public AzureServiceBusTopicHealthCheck(string connectionString, string topicName, ILogger<AzureServiceBusTopicHealthCheck> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _topicName = topicName ?? throw new ArgumentNullException(nameof(topicName));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureServiceBusTopicHealthCheck)} is checking the Azure Topic.");

                var topicClient = new TopicClient(_connectionString, _topicName);

                var scheduledMessageId = await topicClient.ScheduleMessageAsync(
                    new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE)),
                    new DateTimeOffset(DateTime.UtcNow).AddHours(2));

                await topicClient.CancelScheduledMessageAsync(scheduledMessageId);

                _logger?.LogInformation($"The {nameof(AzureServiceBusTopicHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureServiceBusTopicHealthCheck)} check fail for {_connectionString} with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception: ex);
            }
        }
    }
}
