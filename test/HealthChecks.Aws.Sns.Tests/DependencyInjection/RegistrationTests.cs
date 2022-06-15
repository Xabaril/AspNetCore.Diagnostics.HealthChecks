using Amazon;
using Amazon.Runtime;
using HealthChecks.Aws.Sns;

namespace HealthChecks.Aws.SecretsManager.Tests.DependencyInjection;

public class aws_sns_registration_should
{
    [Fact]
    public void add_health_check_with_topics_to_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopics(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.AddTopic("topic1");
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws sns");
        check.GetType().Should().Be(typeof(SnsTopicHealthCheck));
    }

    [Fact]
    public void add_health_check_with_topics_assumerole_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopics(setup =>
            {
                setup.Credentials = new AssumeRoleAWSCredentials(null, "role-arn", "session-name");
                setup.AddTopic("topic1");
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws sns");
        check.GetType().Should().Be(typeof(SnsTopicHealthCheck));
    }

    [Fact]
    public void add_named_health_with_topics_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopics(setup => setup.Credentials = new BasicAWSCredentials("access-key", "secret-key"), name: "awssns");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("awssns");
        check.GetType().Should().Be(typeof(SnsTopicHealthCheck));
    }

    [Fact]
    public void add_health_check_with_topics_when_no_credentials_provided()
    {
        var services = new ServiceCollection();
        var setupCalled = false;

        services.AddHealthChecks()
            .AddSnsTopics(_ => setupCalled = true, name: "awssns");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("awssns");
        check.GetType().Should().Be(typeof(SnsTopicHealthCheck));
        setupCalled.Should().BeTrue();
    }

    [Fact]
    public void add_health_check_with_topics_when_no_credentials_but_region_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopics(setup => setup.RegionEndpoint = RegionEndpoint.EUCentral1);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws sns");
        check.GetType().Should().Be(typeof(SnsTopicHealthCheck));
    }

    [Fact]
    public void add_health_check_with_topics_when_credentials_and_region_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsTopics(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.RegionEndpoint = RegionEndpoint.EUCentral1;
            });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws sns");
        check.GetType().Should().Be(typeof(SnsTopicHealthCheck));
    }


    //Subscriptions
 
    [Fact]
    public void add_health_check_with_topics_and_subscriptions_when_properly_configured1()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsSubscriptions(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.AddTopicAndSubscriptions("topic1", new string[] { "subscription1-arn", "subscription2-arn" });
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws sns subs");
        check.GetType().Should().Be(typeof(SnsSubscriptionHealthCheck));
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_assumerole_when_properly_configured1()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsSubscriptions(setup =>
            {
                setup.Credentials = new AssumeRoleAWSCredentials(null, "role-arn", "session-name");
                setup.AddTopicAndSubscriptions("topic1", new string[] { "subscription1-arn", "subscription2-arn" });
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws sns subs");
        check.GetType().Should().Be(typeof(SnsSubscriptionHealthCheck));
    }

    [Fact]
    public void add_named_health_with_topics_and_subscriptions_check_when_properly_configured1()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsSubscriptions(setup => setup.Credentials = new BasicAWSCredentials("access-key", "secret-key"), name: "awssnssubs");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("awssnssubs");
        check.GetType().Should().Be(typeof(SnsSubscriptionHealthCheck));
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_when_no_credentials_provided1()
    {
        var services = new ServiceCollection();
        var setupCalled = false;

        services.AddHealthChecks()
            .AddSnsSubscriptions(_ => setupCalled = true, name: "awssnssubs");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("awssnssubs");
        check.GetType().Should().Be(typeof(SnsSubscriptionHealthCheck));
        setupCalled.Should().BeTrue();
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_when_no_credentials_but_region_provided1()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsSubscriptions(setup => setup.RegionEndpoint = RegionEndpoint.EUCentral1);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws sns subs");
        check.GetType().Should().Be(typeof(SnsSubscriptionHealthCheck));
    }

    [Fact]
    public void add_health_check_with_topics_and_subscriptions_when_credentials_and_region_provided1()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddSnsSubscriptions(setup =>
            {
                setup.Credentials = new BasicAWSCredentials("access-key", "secret-key");
                setup.RegionEndpoint = RegionEndpoint.EUCentral1;
            });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("aws sns subs");
        check.GetType().Should().Be(typeof(SnsSubscriptionHealthCheck));
    }
}
