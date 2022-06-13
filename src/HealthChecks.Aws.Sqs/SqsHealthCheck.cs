using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sqs;

public class SqsHealthCheck : IHealthCheck
{
    private readonly SqsOptions _sqsOptions;

    public SqsHealthCheck(SqsOptions options)
    {
        _sqsOptions = _sqsOptions ?? throw new ArgumentNullException(nameof(SqsOptions));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSqsClient();
            foreach (var queueName in _sqsOptions.Queues)
            {
                var url = await client.GetQueueUrlAsync(queueName);
                var request = new GetQueueAttributesRequest { QueueUrl = url.QueueUrl };
                request.AttributeNames.Add("ApproximateNumberOfMessages");
                _ = await client.GetQueueAttributesAsync(request, cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
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
