using BeatPulse.Core;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeatPulse.AzureServiceBus
{
    public class AzureServiceBusTopicLiveness : IBeatPulseLiveness
    {
        private const string TEST_MESSAGE = "BeatpulseTest";

        private readonly string _connectionString;
        private readonly string _topicName;
        private readonly ILogger<AzureServiceBusTopicLiveness> _logger;

        public AzureServiceBusTopicLiveness(string connectionString, string topicName, ILogger<AzureServiceBusTopicLiveness> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _topicName = topicName ?? throw new ArgumentNullException(nameof(topicName));
            _logger = logger;
        }

        public async Task<LivenessResult> IsHealthy(LivenessExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureServiceBusTopicLiveness)} is checking the Azure Topic.");

                var topicClient = new TopicClient(_connectionString, _topicName);

                var scheduledMessageId = await topicClient.ScheduleMessageAsync(
                    new Message(Encoding.UTF8.GetBytes(TEST_MESSAGE)),
                    new DateTimeOffset(DateTime.UtcNow).AddHours(2));

                await topicClient.CancelScheduledMessageAsync(scheduledMessageId);

                _logger?.LogInformation($"The {nameof(AzureServiceBusTopicLiveness)} check success.");

                return LivenessResult.Healthy();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureServiceBusTopicLiveness)} check fail for {_connectionString} with the exception {ex.ToString()}.");

                return LivenessResult.UnHealthy(ex);
            }
        }
    }
}
