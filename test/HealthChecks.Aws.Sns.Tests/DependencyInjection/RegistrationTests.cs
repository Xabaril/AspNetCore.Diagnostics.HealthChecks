using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.Sns.Tests.DependencyInjection;

public class aws_sns_registration_should
{
    [Fact]
    public void add_health_check_with_topics_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopicsAndSubscriptions(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.AddTopicAndSubscriptions("topic1");
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sns subs");
        check.ShouldBeOfType<SnsTopicAndSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopicsAndSubscriptions(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.AddTopicAndSubscriptions("topic1", new string[] { "subscription1-arn", "subscription2-arn" });
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sns subs");
        check.ShouldBeOfType<SnsTopicAndSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_topics_assumerole_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopicsAndSubscriptions(setup =>
            {
                setup.Credentials = new AssumeRoleAWSCredentials(null, "role-arn", "session-name");
                setup.AddTopicAndSubscriptions("topic1");
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sns subs");
        check.ShouldBeOfType<SnsTopicAndSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_assumerole_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopicsAndSubscriptions(setup =>
            {
                setup.Credentials = new AssumeRoleAWSCredentials(null, "role-arn", "session-name");
                setup.AddTopicAndSubscriptions("topic1", new string[] { "subscription1-arn", "subscription2-arn" });
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sns subs");
        check.ShouldBeOfType<SnsTopicAndSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_named_health_with_topics_and_subscriptions_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopicsAndSubscriptions(setup => setup.Credentials = new BasicAWSCredentials("access-key", "secret-key"), name: "awssnssubs");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("awssnssubs");
        check.ShouldBeOfType<SnsTopicAndSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_when_no_credentials_provided()
    {
        var services = new ServiceCollection();
        var setupCalled = false;

        services.AddHealthChecks()
            .AddSnsTopicsAndSubscriptions(_ => setupCalled = true, name: "awssnssubs");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("awssnssubs");
        check.ShouldBeOfType<SnsTopicAndSubscriptionHealthCheck>();
        setupCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_when_no_credentials_but_region_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopicsAndSubscriptions(setup => setup.RegionEndpoint = RegionEndpoint.EUCentral1);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sns subs");
        check.ShouldBeOfType<SnsTopicAndSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_when_credentials_and_region_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopicsAndSubscriptions(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.RegionEndpoint = RegionEndpoint.EUCentral1;
            });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("aws sns subs");
        check.ShouldBeOfType<SnsTopicAndSubscriptionHealthCheck>();
    }
}
