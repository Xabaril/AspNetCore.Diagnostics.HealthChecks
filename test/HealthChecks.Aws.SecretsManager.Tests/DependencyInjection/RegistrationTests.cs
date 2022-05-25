using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.SecretsManager.Tests.DependencyInjection;
public class aws_secrets_manager_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSecretsManager(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.AddSecret("supersecret");
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options!.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws secrets manager");
        check.GetType().Should().Be(typeof(SecretsManagerHealthCheck));
    }

    [Fact]
    public void add_health_check_with_assumerole_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSecretsManager(setup =>
            {
                setup.Credentials = new AssumeRoleAWSCredentials(null, "role-arn", "session-name");
                setup.AddSecret("supersecret");
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options!.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws secrets manager");
        check.GetType().Should().Be(typeof(SecretsManagerHealthCheck));
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSecretsManager(setup => setup.Credentials = new BasicAWSCredentials("access-key", "secret-key"), name: "awssecretsmanager");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options!.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("awssecretsmanager");
        check.GetType().Should().Be(typeof(SecretsManagerHealthCheck));
    }

    [Fact]
    public void add_health_check_when_no_credentials_provided()
    {
        var services = new ServiceCollection();
        var setupCalled = false;

        services.AddHealthChecks()
            .AddSecretsManager(_ => setupCalled = true, name: "awssecretsmanager");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options!.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("awssecretsmanager");
        check.GetType().Should().Be(typeof(SecretsManagerHealthCheck));
        setupCalled.Should().BeTrue();
    }

    [Fact]
    public void add_health_check_when_no_credentials_but_region_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSecretsManager(setup => setup.RegionEndpoint = RegionEndpoint.EUCentral1);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options!.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws secrets manager");
        check.GetType().Should().Be(typeof(SecretsManagerHealthCheck));
    }

    [Fact]
    public void add_health_check_when_credentials_and_region_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSecretsManager(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.RegionEndpoint = RegionEndpoint.EUCentral1;
            });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options!.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws secrets manager");
        check.GetType().Should().Be(typeof(SecretsManagerHealthCheck));
    }
}
