using Amazon;

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
}
