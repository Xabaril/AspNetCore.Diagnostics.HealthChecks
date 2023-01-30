using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.Sns;

/// <summary>
/// Options for <see cref="SnsTopicAndSubscriptionHealthCheck"/>.
/// </summary>
public class SnsOptions
{
    public AWSCredentials? Credentials { get; set; }

    public RegionEndpoint? RegionEndpoint { get; set; }

    internal Dictionary<string, List<string>> TopicsAndSubscriptions { get; } = new();


    /// <summary>
    /// Add an AWS SNS topic and its optional subscriptions to be checked
    /// </summary>
    /// <param name="topicName">The topic to be checked.</param>
    /// <param name="subscriptions">The subscription ARNs from the <paramref name="topicName"/> to be checked.</param>
    /// <returns>Reference to the same <see cref="SnsOptions"/> to allow further configuration.</returns>
    public SnsOptions AddTopicAndSubscriptions(string topicName, IEnumerable<string>? subscriptions = null)
    {
        if (!TopicsAndSubscriptions.TryGetValue(topicName, out var subs))
        {
            TopicsAndSubscriptions.Add(topicName, subs = new List<string>(subscriptions ?? Array.Empty<string>()));
        }
        else if (subscriptions != null)
        {
            subs.AddRange(subscriptions);
        }
        return this;
    }
}
