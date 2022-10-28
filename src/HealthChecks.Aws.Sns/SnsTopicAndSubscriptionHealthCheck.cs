using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sns;

public class SnsTopicAndSubscriptionHealthCheck : IHealthCheck
{
    private readonly SnsOptions _snsOptions;

    public SnsTopicAndSubscriptionHealthCheck(SnsOptions snsOptions)
    {
        _snsOptions = Guard.ThrowIfNull(snsOptions);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSnsClient();

            foreach (var (topicName, subscriptions) in _snsOptions.TopicsAndSubscriptions.Select(x => (x.Key, x.Value)))
            {
                var topic = await client.FindTopicAsync(topicName);

                if (topic == null)
                {
                    throw new NotFoundException($"Topic {topicName} does not exist.");
                }

                if (subscriptions.Count == 0)
                {
                    continue;
                }

                var subscriptionsFromAws = await client.ListSubscriptionsByTopicAsync(topic.TopicArn, cancellationToken);

                var subscriptionsArn = subscriptionsFromAws.Subscriptions.Select(s => s.SubscriptionArn);

                foreach (var subscription in subscriptions)
                {
                    if (!subscriptionsArn.Contains(subscription))
                    {
                        throw new NotFoundException($"Subscription {subscription} in Topic {topicName} does not exist.");
                    }
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
