using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System;

namespace HealthChecks.Aws.S3
{
    public class S3BucketOptions
    {
        public AWSCredentials? Credentials { get; set; }

        [Obsolete("Specify access key and secret as a BasicCredential to Credentials property instead")]
        public string? AccessKey { get; set; }

        [Obsolete("Specify access key and secret as a BasicCredential to Credentials property instead")]
        public string? SecretKey { get; set; }

        public AmazonS3Config S3Config { get; set; } = null!;

        public string BucketName { get; set; } = null!;

        public Func<ListObjectsResponse, bool>? CustomResponseCheck { get; set; }
    }
}
