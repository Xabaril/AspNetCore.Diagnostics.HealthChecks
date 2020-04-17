using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Aws.S3
{
    public class S3HealthCheck : IHealthCheck
    {
        private readonly S3BucketOptions _bucketOptions;
        public S3HealthCheck(S3BucketOptions bucketOptions)
        {
            if (bucketOptions == null)
            {
                throw new ArgumentNullException(nameof(bucketOptions));
            }
            if (bucketOptions.S3Config == null)
            {
                throw new ArgumentNullException(nameof(S3BucketOptions.S3Config));
            }
            _bucketOptions = bucketOptions;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                bool keysProvided = !string.IsNullOrEmpty(_bucketOptions.AccessKey) &&
                                    !string.IsNullOrEmpty(_bucketOptions.SecretKey);
                
                AmazonS3Client client = keysProvided
                    ? new AmazonS3Client(_bucketOptions.AccessKey, _bucketOptions.SecretKey, _bucketOptions.S3Config)
                    : new AmazonS3Client(_bucketOptions.S3Config);
                
                using (client)
                {
                    var response = await client.ListObjectsAsync(_bucketOptions.BucketName, cancellationToken);

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
}
