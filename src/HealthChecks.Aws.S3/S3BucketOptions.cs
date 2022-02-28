using Amazon.S3;
using Amazon.S3.Model;

namespace HealthChecks.Aws.S3
{
    public class S3BucketOptions
    {
        public string? AccessKey { get; set; }

        public string? SecretKey { get; set; }

        public AmazonS3Config S3Config { get; set; } = null!;

        public string BucketName { get; set; } = null!;

        public Func<ListObjectsResponse, bool>? CustomResponseCheck { get; set; }
    }
}
