using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sns;

public class SnsTopicAndSubscriptionHealthCheck : IHealthCheck
{
    private readonly SnsOptions _options;


    public SnsTopicAndSubscriptionHealthCheck(SnsOptions snsOptions)
    {
        _options = Guard.ThrowIfNull(snsOptions);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSnsClient();

            foreach (var (topicName, subscriptions) in _options.TopicsAndSubscriptions.Select(x => (x.Key, x.Value)))
            {
                var topic = await client.FindTopicAsync(topicName).ConfigureAwait(false)
                    ?? throw new NotFoundException($"Topic {topicName} does not exist.");

                if (subscriptions.Count == 0)
                {
                    continue;
                }

                var subscriptionsFromAws = await client.ListSubscriptionsByTopicAsync(topic.TopicArn, cancellationToken).ConfigureAwait(false);

                var subscriptionsArn = subscriptionsFromAws.Subscriptions.Select(s => s.SubscriptionArn);

                foreach (string? subscription in subscriptions)
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
        bool credentialsProvided = _options.Credentials is not null;
        bool regionProvided = _options.RegionEndpoint is not null;

        var config = new AmazonSimpleNotificationServiceConfig();

        if (_options.ServiceURL is not null)
        {
            config.ServiceURL = _options.ServiceURL;
        }
        if (_options.RegionEndpoint is not null)
        {
            config.RegionEndpoint = _options.RegionEndpoint;
        }

        return _options.Credentials is not null
            ? new AmazonSimpleNotificationServiceClient(_options.Credentials, config)
            : new AmazonSimpleNotificationServiceClient(config);
    }
}
