using Amazon.S3;
using Amazon.Util;

namespace HealthChecks.Aws.S3.Tests.DependencyInjection;

public class aws_s3_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddS3(options =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                options.AccessKey = "access-key";
                options.BucketName = "bucket-name";
                options.SecretKey = "secret-key";
                options.S3Config = new AmazonS3Config();
#pragma warning restore CS0618 // Type or member is obsolete
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws s3");
        check.ShouldBeOfType<S3HealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_2()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddS3(_ => _.S3Config = new AmazonS3Config());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws s3");
        check.ShouldBeOfType<S3HealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
             .AddS3(options =>
             {
#pragma warning disable CS0618 // Type or member is obsolete
                 options.AccessKey = "access-key";
                 options.BucketName = "bucket-name";
                 options.SecretKey = "secret-key";
                 options.S3Config = new AmazonS3Config();
#pragma warning restore CS0618 // Type or member is obsolete
             }, name: "aws s3 check");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws s3 check");
        check.ShouldBeOfType<S3HealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured_2()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddS3(_ => _.S3Config = new AmazonS3Config(), name: "my-s3-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-s3-group");
        check.ShouldBeOfType<S3HealthCheck>();
    }


    /// <summary>
    /// Interface representing some potential configuration class to be used by the AddS3 Health Check class.
    /// </summary>
    private interface IS3Config
    {
        string ServiceUrl { get; set; }
        string AccessKey { get; set; }
        string SecretKey { get; set; }

    }

    private class S3Config : IS3Config
    {
        public required string ServiceUrl { get; set; }
        public required string AccessKey { get; set; }
        public required string SecretKey { get; set; }
    }


    [Fact]
    public void add_health_check_when_properly_configured_with_serviceprovider()
    {
        var services = new ServiceCollection();
        services.AddTransient<IS3Config>(p => new S3Config
        {
            ServiceUrl = "http://localhost:9000",
            AccessKey = "access-key",
            SecretKey = "secret-key"
        });

        services.AddHealthChecks()
            .AddS3((sp, options) =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var cfg = sp.GetRequiredService<IS3Config>();
                options.AccessKey = cfg.AccessKey;
                options.BucketName = "bucket-name";
                options.SecretKey = cfg.SecretKey;
                options.S3Config = new AmazonS3Config
                {
                    ServiceURL = cfg.ServiceUrl
                };
#pragma warning restore CS0618 // Type or member is obsolete
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws s3 with sp");
        check.ShouldBeOfType<S3HealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured_with_serviceprovider()
    {
        var services = new ServiceCollection();
        services.AddTransient<IS3Config>(p => new S3Config
        {
            ServiceUrl = "http://localhost:9000",
            AccessKey = "access-key",
            SecretKey = "secret-key"
        });

        services.AddHealthChecks()
             .AddS3((sp, options) =>
             {
#pragma warning disable CS0618 // Type or member is obsolete
                 var cfg = sp.GetRequiredService<IS3Config>();
                 options.AccessKey = cfg.AccessKey;
                 options.BucketName = "bucket-name";
                 options.SecretKey = cfg.SecretKey;
                 options.S3Config = new AmazonS3Config
                 {
                     ServiceURL = cfg.ServiceUrl
                 };
#pragma warning restore CS0618 // Type or member is obsolete
             }, name: "aws s3 check");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws s3 check");
        check.ShouldBeOfType<S3HealthCheck>();
    }



}
