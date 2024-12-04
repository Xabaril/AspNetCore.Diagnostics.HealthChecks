namespace HealthChecks.SendGrid.Tests.DependencyInjection;

public class sendgrid_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSendGrid("wellformed_but_invalid_token");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("sendgrid");
        check.ShouldBeOfType<SendGridHealthCheck>();
    }

    private class SendGridOptions
    {
        public string ApiKey { get; set; } = default!;
    }

    [Fact]
    public void add_health_check_from_DI_when_properly_configured()
    {
        bool called = false;
        var services = new ServiceCollection();

        services.AddOptions<SendGridOptions>().Configure(options => options.ApiKey = "my_api_key");
        services.AddHealthChecks()
            .AddSendGrid(sp =>
            {
                called = true;
                return sp.GetRequiredService<IOptions<SendGridOptions>>().Value.ApiKey;
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("sendgrid");
        check.ShouldBeOfType<SendGridHealthCheck>();
        called.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSendGrid("wellformed_but_invalid_token", "my-sendgrid-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-sendgrid-group");
        check.ShouldBeOfType<SendGridHealthCheck>();
    }
}
