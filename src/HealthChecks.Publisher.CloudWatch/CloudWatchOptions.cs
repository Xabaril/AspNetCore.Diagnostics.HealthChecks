using Amazon;
using Amazon.CloudWatch;

namespace HealthChecks.Publisher.CloudWatch;

/// <summary>
/// Options for AWS CloudWatch publisher.
/// </summary>
public class CloudWatchOptions
{
    public string? ServiceCheckName { get; set; }

    public string? AwsAccessKeyId { get; set; }

    public string? AwsSecretAccessKey { get; set; }

    public RegionEndpoint? Region { get; set; }

    /// <summary>
    /// The namespace for the metric data.
    /// </summary>
    public string Namespace { get; set; } = "Xabaril/AspNetCoreDiagnosticsHealthChecks";

    /// <summary>
    /// Delegate to build <see cref="AmazonCloudWatchClient"/> used by AWS CloudWatch publisher.
    /// </summary>
    public Func<CloudWatchOptions, AmazonCloudWatchClient> ClientBuilder { get; set; } = options =>
    {
        return options.AwsAccessKeyId is null && options.AwsSecretAccessKey is null && options.Region is null
            ? new AmazonCloudWatchClient()
            : new AmazonCloudWatchClient(options.AwsAccessKeyId, options.AwsSecretAccessKey, options.Region);
    };
}
