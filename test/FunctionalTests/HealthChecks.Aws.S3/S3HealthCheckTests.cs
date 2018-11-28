using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.Aws.S3.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FunctionalTests.HealthChecks.Aws.S3
{
    [Collection("execution")]
    public class s3_healthcheck_should
    {
        private const string AccessKeyForLocalStack = "Any access key for local stack";
        private const string SecretKeyForLocalStack = "Any secret key for local stack";
        private readonly ExecutionFixture _fixture;

        private readonly AmazonS3Config _localStackS3Config = new AmazonS3Config
        {
            ServiceURL = "http://localhost:4572",
            UseHttp = true,
            ForcePathStyle = true
        };

        private readonly AmazonS3Client _s3Client;

        public s3_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _s3Client = new AmazonS3Client(_localStackS3Config);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_bucket_exists()
        {
            await GivenS3Bucket("my-other-bucket");

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddS3(options =>
                        {
                            options.BucketName = "my-other-bucket";
                            options.AccessKey = AccessKeyForLocalStack;
                            options.SecretKey = SecretKeyForLocalStack;
                            options.S3Config = _localStackS3Config;
                        });
                })
                .Configure(app => { app.UseHealthChecks("/health"); });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_if_bucket_not_exists()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddS3(options =>
                        {
                            options.BucketName = "non-existing-bucket";
                            options.AccessKey = AccessKeyForLocalStack;
                            options.SecretKey = SecretKeyForLocalStack;
                            options.S3Config = _localStackS3Config;
                        });
                })
                .Configure(app => { app.UseHealthChecks("/health"); });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_if_file_is_available()
        {
            const string bucketName = "test-bucket";
            await GivenS3Bucket(bucketName);
            await GivenFileInS3Bucket(bucketName, "awesome.txt");

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddS3(options =>
                        {
                            options.BucketName = bucketName;
                            options.AccessKey = AccessKeyForLocalStack;
                            options.SecretKey = SecretKeyForLocalStack;
                            options.S3Config = _localStackS3Config;
                            options.CustomResponseCheck = objectsResponse =>
                                objectsResponse.S3Objects.Any(_ => _.Key == "awesome.txt");
                        });
                })
                .Configure(app => { app.UseHealthChecks("/health"); });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }


        [SkipOnAppVeyor]
        public async Task be_unhealthy_if_file_not_found()
        {
            await GivenS3Bucket("test-bucket");

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddS3(options =>
                        {
                            options.BucketName = "test-bucket";
                            options.AccessKey = AccessKeyForLocalStack;
                            options.SecretKey = SecretKeyForLocalStack;
                            options.S3Config = _localStackS3Config;
                            options.CustomResponseCheck = objectsResponse =>
                                objectsResponse.S3Objects.Any(_ => _.Key == "not_there.txt");
                        });
                })
                .Configure(app => { app.UseHealthChecks("/health"); });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }


        private async Task GivenS3Bucket(string bucketName)
        {
            await _s3Client.PutBucketAsync(bucketName);
        }

        private async Task GivenFileInS3Bucket(string bucketName, string fileKey)
        {
            using (var ms = new MemoryStream())
            {
                TextWriter tw = new StreamWriter(ms);
                await tw.WriteLineAsync("Some content");
                tw.Flush();
                ms.Position = 0;
                using (var fileTransferUtility = new TransferUtility(_s3Client))
                {
                    await fileTransferUtility.UploadAsync(ms, bucketName, fileKey);
                }
            }
        }
    }
}