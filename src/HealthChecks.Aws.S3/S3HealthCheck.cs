using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.S3;

public class S3HealthCheck : IHealthCheck
{
    private readonly S3BucketOptions _bucketOptions;

    public S3HealthCheck(S3BucketOptions bucketOptions)
    {
        Guard.ThrowIfNull(bucketOptions);
        Guard.ThrowIfNull(bucketOptions.S3Config);

        _bucketOptions = bucketOptions;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            AWSCredentials? credentials = _bucketOptions.Credentials;

            if (credentials == null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (!string.IsNullOrEmpty(_bucketOptions.AccessKey) && !string.IsNullOrEmpty(_bucketOptions.SecretKey))
                {
                    // for backwards compatibility we create the basic credentials if the old fields are used
                    // but if they are not specified we fallback to using the default profile
                    credentials = new BasicAWSCredentials(_bucketOptions.AccessKey, _bucketOptions.SecretKey);
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }

            var client = credentials != null
                ? new AmazonS3Client(credentials, _bucketOptions.S3Config)
                : new AmazonS3Client(_bucketOptions.S3Config);

            using (client)
            {
                var listRequest = new ListObjectsRequest
                {
                    BucketName = _bucketOptions.BucketName, MaxKeys = _bucketOptions.MaxKeys
                };
                var response = await client.ListObjectsAsync(_bucketOptions.BucketName, cancellationToken).ConfigureAwait(false);

                if (_bucketOptions.CustomResponseCheck != null)
                {
                    return _bucketOptions.CustomResponseCheck.Invoke(response)
                        ? HealthCheckResult.Healthy()
                        : new HealthCheckResult(context.Registration.FailureStatus, description: "Custom response check is not satisfied.");
                }
            }
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
