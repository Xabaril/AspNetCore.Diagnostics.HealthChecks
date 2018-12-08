using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
                var topicClient = new TopicClient(_connectionString, _topicName);

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
