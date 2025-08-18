using Amazon.SQS;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sqs;

public class SqsHealthCheck : IHealthCheck
{
    private readonly SqsOptions _sqsOptions;

    public SqsHealthCheck(SqsOptions sqsOptions)
    {
        _sqsOptions = Guard.ThrowIfNull(sqsOptions);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var currentQueue = "";
        var checkDetails = new Dictionary<string, object>{
            { "health_check.task", "ready" },
            { "messaging.system", "aws.sqs" }
        };

        try
        {
            using var client = CreateSqsClient();
            foreach (var queueName in _sqsOptions.Queues)
            {
                currentQueue = queueName;
                _ = await client.GetQueueUrlAsync(queueName).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy(data: checkDetails);
        }
        catch (Exception ex)
        {
            checkDetails.Add("messaging.destination.name", currentQueue);
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }

    private IAmazonSQS CreateSqsClient()
    {
        var credentialsProvided = _sqsOptions.Credentials is not null;
        var regionProvided = _sqsOptions.RegionEndpoint is not null;
        return (credentialsProvided, regionProvided) switch
        {
            (false, false) => new AmazonSQSClient(),
            (false, true) => new AmazonSQSClient(_sqsOptions.RegionEndpoint),
            (true, false) => new AmazonSQSClient(_sqsOptions.Credentials),
            (true, true) => new AmazonSQSClient(_sqsOptions.Credentials, _sqsOptions.RegionEndpoint)
        };
    }
}
