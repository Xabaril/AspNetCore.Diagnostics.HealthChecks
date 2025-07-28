using System.Collections.ObjectModel;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sns;

public class SnsTopicAndSubscriptionHealthCheck : IHealthCheck
{
    private readonly SnsOptions _snsOptions;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                { "health_check.task", "ready" },
                { "messaging.system", "aws.sns" }
    };

    public SnsTopicAndSubscriptionHealthCheck(SnsOptions snsOptions)
    {
        _snsOptions = Guard.ThrowIfNull(snsOptions);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var currentTopic = "";
        var currentSubscription = "";
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            using var client = CreateSnsClient();

            foreach (var (topicName, subscriptions) in _snsOptions.TopicsAndSubscriptions.Select(x => (x.Key, x.Value)))
            {
                currentTopic = topicName;
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
                    currentSubscription = subscription;
                    if (!subscriptionsArn.Contains(subscription))
                    {
                        throw new NotFoundException($"Subscription {subscription} in Topic {topicName} does not exist.");
                    }
                }
            }

            return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            checkDetails.Add("messaging.destination.name", currentTopic);
            checkDetails.Add("messaging.destination.subscription.name", currentSubscription);
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }

    private AmazonSimpleNotificationServiceClient CreateSnsClient()
    {
        bool credentialsProvided = _snsOptions.Credentials is not null;
        bool regionProvided = _snsOptions.RegionEndpoint is not null;
        return (credentialsProvided, regionProvided) switch
        {
            (false, false) => new AmazonSimpleNotificationServiceClient(),
            (false, true) => new AmazonSimpleNotificationServiceClient(_snsOptions.RegionEndpoint),
            (true, false) => new AmazonSimpleNotificationServiceClient(_snsOptions.Credentials),
            (true, true) => new AmazonSimpleNotificationServiceClient(_snsOptions.Credentials, _snsOptions.RegionEndpoint)
        };
    }
}
