using System.Diagnostics.CodeAnalysis;
using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.Sqs;

/// <summary>
/// Options for <see cref="SqsHealthCheck"/>.
/// </summary>
public class SqsOptions
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public string? ServiceURL { get; set; }

    public AWSCredentials? Credentials { get; set; }

    public RegionEndpoint? RegionEndpoint { get; set; }

    internal HashSet<string> Queues { get; } = [];

    /// <summary>
    /// Add an AWS SQS queue to be checked.
    /// </summary>
    /// <param name="queueName">The queue to be checked.</param>
    /// <returns>Reference to the same <see cref="SqsOptions"/> to allow further configuration.</returns>
    public SqsOptions AddQueue(string queueName)
    {
        Queues.Add(queueName);

        return this;
    }
}
