using Amazon;

namespace HealthChecks.DynamoDb.Tests.DependencyInjection;

public class dynamoDb_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();

#pragma warning disable CS0618 // Type or member is obsolete
        services.AddHealthChecks()
            .AddDynamoDb(_ => { _.AccessKey = "key"; _.SecretKey = "key"; _.RegionEndpoint = RegionEndpoint.CNNorth1; });
#pragma warning restore CS0618 // Type or member is obsolete

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("dynamodb");
        check.ShouldBeOfType<DynamoDbHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();

#pragma warning disable CS0618 // Type or member is obsolete
        services.AddHealthChecks()
            .AddDynamoDb(_ => { _.AccessKey = "key"; _.SecretKey = "key"; _.RegionEndpoint = RegionEndpoint.CNNorth1; }, name: "my-dynamodb-group");
#pragma warning restore CS0618 // Type or member is obsolete

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-dynamodb-group");
        check.ShouldBeOfType<DynamoDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_no_credentials_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddDynamoDb(_ => _.RegionEndpoint = RegionEndpoint.CNNorth1);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("dynamodb");
        check.ShouldBeOfType<DynamoDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_no_option_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddDynamoDb();

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("dynamodb");
        check.ShouldBeOfType<DynamoDbHealthCheck>();
    }
}
