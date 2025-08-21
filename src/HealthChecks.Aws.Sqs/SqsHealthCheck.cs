using Amazon.SQS;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sqs;

public class SqsHealthCheck : IHealthCheck
{
    private readonly SqsOptions _options;

    public SqsHealthCheck(SqsOptions sqsOptions)
    {
        _options = Guard.ThrowIfNull(sqsOptions);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSqsClient();

            foreach (string queueName in _options.Queues)
            {
                await client.GetQueueUrlAsync(queueName, cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: e);
        }
    }

    private AmazonSQSClient CreateSqsClient()
    {
        var config = new AmazonSQSConfig();

        if (_options.ServiceURL is not null)
        {
            config.ServiceURL = _options.ServiceURL;
        }

        if (_options.RegionEndpoint is not null)
        {
            config.RegionEndpoint = _options.RegionEndpoint;
        }

        return _options.Credentials is not null
            ? new AmazonSQSClient(_options.Credentials, config)
            : new AmazonSQSClient(config);
    }
}
