using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.Sqs.Tests.DependencyInjection;

public class aws_sqs_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSqs(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.AddQueue("queue");
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sqs");
        check.ShouldBeOfType<SqsHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_assumerole_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSqs(setup =>
            {
                setup.Credentials = new AssumeRoleAWSCredentials(null, "role-arn", "session-name");
                setup.AddQueue("queue");
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sqs");
        check.ShouldBeOfType<SqsHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSqs(setup => setup.Credentials = new BasicAWSCredentials("access-key", "secret-key"), name: "awssqs");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("awssqs");
        check.ShouldBeOfType<SqsHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_no_credentials_provided()
    {
        var services = new ServiceCollection();
        var setupCalled = false;

        services.AddHealthChecks()
            .AddSqs(_ => setupCalled = true, name: "awssqs");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("awssqs");
        check.ShouldBeOfType<SqsHealthCheck>();
        setupCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_when_no_credentials_but_region_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSqs(setup => setup.RegionEndpoint = RegionEndpoint.EUCentral1);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sqs");
        check.ShouldBeOfType<SqsHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_credentials_and_region_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSqs(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.RegionEndpoint = RegionEndpoint.EUCentral1;
            });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sqs");
        check.ShouldBeOfType<SqsHealthCheck>();
    }
}
