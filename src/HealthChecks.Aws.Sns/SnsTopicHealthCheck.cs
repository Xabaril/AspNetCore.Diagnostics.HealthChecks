using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sns
{
    public class SnsTopicHealthCheck : IHealthCheck
    {
        private readonly SnsOptions _snsOptions;

        public SnsTopicHealthCheck(SnsOptions snsOptions)
        {
            _snsOptions = snsOptions ?? throw new ArgumentNullException(nameof(SnsOptions));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = CreateSnsClient();

                foreach (var topicName in _snsOptions.Topics)
                {
                    var topic = await client.FindTopicAsync(topicName);

                    if (topic == null)
                    {
                        throw new NotFoundException($"Topic {topicName} does not exist.");
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private AmazonSimpleNotificationServiceClient CreateSnsClient()
        {
            var credentialsProvided = _snsOptions.Credentials is not null;
            var regionProvided = _snsOptions.RegionEndpoint is not null;
            return (credentialsProvided, regionProvided) switch
            {
                (false, false) => new AmazonSimpleNotificationServiceClient(),
                (false, true) => new AmazonSimpleNotificationServiceClient(_snsOptions.RegionEndpoint),
                (true, false) => new AmazonSimpleNotificationServiceClient(_snsOptions.Credentials),
                (true, true) => new AmazonSimpleNotificationServiceClient(_snsOptions.Credentials, _snsOptions.RegionEndpoint)
            };
        }
    }
}
