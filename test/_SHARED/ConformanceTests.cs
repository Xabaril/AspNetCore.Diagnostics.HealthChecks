using System.Net;

public abstract class ConformanceTests<TClient, THealthCheck, THealthCheckOptions>
    where TClient : class
    where THealthCheck : IHealthCheck
    where THealthCheckOptions : class
{
    protected abstract TClient CreateClientForNonExistingEndpoint();

    protected abstract THealthCheckOptions CreateHealthCheckOptions();

    protected abstract THealthCheck CreateHealthCheck(TClient client, THealthCheckOptions? options);

    protected abstract IHealthChecksBuilder AddHealthCheck(
        IHealthChecksBuilder builder,
        Func<IServiceProvider, TClient>? clientFactory = default,
        Func<IServiceProvider, THealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default);

    [Fact]
    public void HealthCheckTypeIsSealed() => Assert.True(typeof(THealthCheck).IsSealed);

    [Fact]
    public void OptionsTypeIsSealed() => Assert.True(typeof(THealthCheckOptions).IsSealed);

    [Fact]
    public void CtorThrowsArgumentNullExceptionForNullClient()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => CreateHealthCheck(client: null!, options: CreateHealthCheckOptions()));

        Assert.False(string.IsNullOrEmpty(argumentNullException.ParamName));
    }

    [Fact]
    public void CtorAcceptsNullOptions()
    {
        TClient client = CreateClientForNonExistingEndpoint();

        Assert.NotNull(client);

        CreateHealthCheck(client: client, options: null!);
    }

    [Theory]
    [InlineData(HealthStatus.Unhealthy, true)]
    [InlineData(HealthStatus.Unhealthy, false)]
    [InlineData(HealthStatus.Degraded, true)]
    [InlineData(HealthStatus.Degraded, false)]
    public async Task ReturnsProvidedFailureStatusWhenConnectionCanNotBeMade(HealthStatus failureStatus, bool useDiExtension)
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                if (useDiExtension)
                {
                    services.AddSingleton(sp => CreateClientForNonExistingEndpoint());
                    AddHealthCheck(builder: services.AddHealthChecks(), failureStatus: failureStatus);
                }
                else
                {
                    TClient client = CreateClientForNonExistingEndpoint();

                    services.AddHealthChecks()
                        .Add(new HealthCheckRegistration(
                            name: "name",
                            instance: CreateHealthCheck(client, null),
                            failureStatus: failureStatus,
                            tags: null));
                }
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                });
            });

        using TestServer server = new(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(failureStatus == HealthStatus.Unhealthy ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(HealthStatus.Unhealthy)]
    [InlineData(HealthStatus.Healthy)]
    public async Task DependencyInjectionRegistrationWorksAsExpected(HealthStatus failureStatus)
    {
        const string healthCheckName = "random_name";
        var timeout = TimeSpan.FromSeconds(5);
        string[] tags = { "a", "b", "c" };
        int counter = 0;

        ServiceCollection services = new();

        services.AddSingleton(_ =>
        {
            counter++;

            return CreateClientForNonExistingEndpoint();
        });

        AddHealthCheck(builder: services.AddHealthChecks(),
            healthCheckName: healthCheckName,
            failureStatus: failureStatus,
            tags: tags,
            timeout: timeout);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IOptions<HealthCheckServiceOptions> options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        HealthCheckRegistration registration = options.Value.Registrations.Single();

        counter.ShouldBe(0);

        IHealthCheck check = registration.Factory(serviceProvider);
        check.ShouldBeOfType<THealthCheck>();

        counter.ShouldBe(1);

        registration.Name.ShouldBe(healthCheckName);
        registration.FailureStatus.ShouldBe(failureStatus);
        registration.Tags.ToArray().ShouldBeEquivalentTo(tags);
        registration.Timeout.ShouldBe(timeout);

        for (int i = 0; i < 10; i++)
        {
            registration.Factory(serviceProvider);
        }

        counter.ShouldBe(1);
    }
}
