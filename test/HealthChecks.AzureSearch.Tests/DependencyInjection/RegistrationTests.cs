using HealthChecks.AzureSearch.DependencyInjection;

namespace HealthChecks.AzureSearch.Tests.DependencyInjection;

public class azuresearch_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureSearch(_ => { _.Endpoint = "endpoint"; _.IndexName = "indexName"; _.AuthKey = "authKey"; });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresearch");
        check.ShouldBeOfType<AzureSearchHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureSearch(_ => { _.Endpoint = "endpoint"; _.IndexName = "indexName"; _.AuthKey = "authKey"; }, name: "my-azuresearch-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-azuresearch-group");
        check.ShouldBeOfType<AzureSearchHealthCheck>();
    }
}
