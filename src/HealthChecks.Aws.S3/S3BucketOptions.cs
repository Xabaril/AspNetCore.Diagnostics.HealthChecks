using Amazon.S3;
using Amazon.S3.Model;
using System;

namespace HealthChecks.Aws.S3
{
    public class S3BucketOptions
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public AmazonS3Config S3Config { get; set; }
        public string BucketName { get; set; }
        public Func<ListObjectsResponse, bool> CustomResponseCheck { get; set; }
    }
}
