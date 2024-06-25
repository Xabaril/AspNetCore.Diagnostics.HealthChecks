namespace HealthChecks.Elasticsearch.Tests.DependencyInjection;

public class elasticsearch_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddElasticsearch("uri");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("elasticsearch");
        check.ShouldBeOfType<ElasticsearchHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddElasticsearch("uri", name: "my-elasticsearch");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-elasticsearch");
        check.ShouldBeOfType<ElasticsearchHealthCheck>();
    }

    [Fact]
    public void create_client_with_user_configured_request_timeout()
    {
        var connectionString = @"https://localhost:9200";

        var services = new ServiceCollection();
        var settings = new ElasticsearchOptions();
        services.AddHealthChecks().AddElasticsearch(setup =>
        {
            setup.UseServer(connectionString);
            setup.RequestTimeout = new TimeSpan(0, 0, 6);
            settings = setup;
        });

        //Ensure no further modifications were carried by extension method
        settings.RequestTimeout.ShouldNotBeNull();
        settings.RequestTimeout.ShouldBe(new TimeSpan(0, 0, 6));
    }

    [Fact]
    public void create_client_with_configured_healthcheck_timeout_when_no_request_timeout_is_configured()
    {
        var connectionString = @"https://localhost:9200";
        var services = new ServiceCollection();
        var settings = new ElasticsearchOptions();

        services.AddHealthChecks().AddElasticsearch(setup =>
        {
            setup.UseServer(connectionString);
            settings = setup;
        }, timeout: new TimeSpan(0, 0, 7));

        settings.RequestTimeout.ShouldNotBeNull();
        settings.RequestTimeout.ShouldBe(new TimeSpan(0, 0, 7));
    }

    [Fact]
    public void create_client_with_no_timeout_when_no_option_is_configured()
    {
        var connectionString = @"https://localhost:9200";

        var services = new ServiceCollection();
        var settings = new ElasticsearchOptions();

        services.AddHealthChecks().AddElasticsearch(setup =>
        {
            setup.UseServer(connectionString);
            settings = setup;
        });

        settings.RequestTimeout.ShouldBeNull();
    }

    [Fact]
    public void throw_exception_when_create_client_without_using_elasic_cloud_or_server()
    {

        var services = new ServiceCollection();
        var settings = new ElasticsearchOptions();

        Assert.Throws<InvalidOperationException>(() => services.AddHealthChecks().AddElasticsearch(setup => settings = setup));
    }
    [Fact]
    public void create_client_when_using_elasic_cloud()
    {

        var services = new ServiceCollection();
        var settings = new ElasticsearchOptions();

        services.AddHealthChecks().AddElasticsearch(setup =>
        {
            setup.UseElasticCloud("cloudId", "cloudApiKey");
            settings = setup;
        });

        settings.AuthenticateWithElasticCloud.ShouldBeTrue();
        settings.CloudApiKey.ShouldNotBeNull();
        settings.CloudId.ShouldNotBeNull();
    }

    [Fact]
    public void client_should_resolve_from_di()
    {
        var client = new Elastic.Clients.Elasticsearch.ElasticsearchClient();
        var services = new ServiceCollection();
        var settings = new ElasticsearchOptions();
        services.AddSingleton(client);

        services.AddHealthChecks().AddElasticsearch(clientFactory: null, setup: (setup) => settings = setup);

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<ElasticsearchHealthCheck>();
        settings.Client.ShouldNotBeNull();
        settings.Client.ShouldBe(client);
    }
    [Fact]
    public void use_client_factory_should_use_same_client()
    {
        var client = new Elastic.Clients.Elasticsearch.ElasticsearchClient();
        var services = new ServiceCollection();
        var settings = new ElasticsearchOptions();

        services.AddHealthChecks().AddElasticsearch(clientFactory: (sp => client), setup: (setup) => settings = setup);

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<ElasticsearchHealthCheck>();

        settings.Client.ShouldNotBeNull();
        settings.Client.ShouldBe(client);
    }
}
