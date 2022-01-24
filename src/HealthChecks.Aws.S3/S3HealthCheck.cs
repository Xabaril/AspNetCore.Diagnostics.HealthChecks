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
        private readonly IAmazonS3 _amazonS3;

        public S3HealthCheck(S3BucketOptions bucketOptions, IAmazonS3 amazonS3)
        {
            if (bucketOptions == null)
            {
                throw new ArgumentNullException(nameof(bucketOptions));
            }
    
            _bucketOptions = bucketOptions;
            _amazonS3 = amazonS3;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {

                using (_amazonS3)
                {
                    var response = await _amazonS3.ListObjectsAsync(_bucketOptions.BucketName, cancellationToken);

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
