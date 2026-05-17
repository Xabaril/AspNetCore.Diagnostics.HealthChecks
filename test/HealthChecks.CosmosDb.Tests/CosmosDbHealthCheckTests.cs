using System.Net;
using Azure;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.CosmosDb.Tests;

public class cosmosdbhealthcheck_should
{
    private static readonly string[] ContainerIds = ["one", "two", "three"];
    private const string DatabaseId = "unit-test";
    private const string HealthCheckName = "unit-test-check";

    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly IReadOnlyDictionary<string, Container> _containers;
    private readonly AzureCosmosDbHealthCheckOptions _options;
    private readonly AzureCosmosDbHealthCheck _healthCheck;
    private readonly HealthCheckContext _context;

    public cosmosdbhealthcheck_should()
    {
        _cosmosClient = Substitute.For<CosmosClient>();
        _database = Substitute.For<Database>();
        _options = new AzureCosmosDbHealthCheckOptions();
        _healthCheck = new AzureCosmosDbHealthCheck(_cosmosClient, _options);
        _context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, _healthCheck, HealthStatus.Unhealthy, null)
        };

        _cosmosClient.GetDatabase(DatabaseId).Returns(_database);
        _containers = ContainerIds
            .Select(x => KeyValuePair.Create(x, Substitute.For<Container>()))
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var pair in _containers)
        {
            _database.GetContainer(pair.Key).Returns(pair.Value);
        }
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service()
    {
        using var tokenSource = new CancellationTokenSource();

        _cosmosClient
            .ReadAccountAsync()
            .Returns(Substitute.For<AccountProperties>());

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync();

        await _database
            .DidNotReceiveWithAnyArgs()
            .ReadAsync(default);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)));

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_and_database()
    {
        using var tokenSource = new CancellationTokenSource();

        _cosmosClient
            .ReadAccountAsync()
            .Returns(Substitute.For<AccountProperties>());

        _database
            .ReadAsync(null, tokenSource.Token)
            .Returns(Substitute.For<DatabaseResponse>());

        _options.DatabaseId = DatabaseId;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync();

        await _database
            .Received(1)
            .ReadAsync(null, tokenSource.Token);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)));

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_database_and_containers()
    {
        using var tokenSource = new CancellationTokenSource();

        _options.ContainerIds = ContainerIds;
        _options.DatabaseId = DatabaseId;

        for (var containerIndex = 0; containerIndex < _options.ContainerIds.Count(); containerIndex++)
        {
            _cosmosClient.GetContainer(_options.DatabaseId, _options.ContainerIds.ElementAt(containerIndex)).Returns(_containers[ContainerIds[containerIndex]]);
        }

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        for (int containerIndex = 0; containerIndex < _options.ContainerIds.Count(); containerIndex++)
        {
            _cosmosClient.Received(1)
                .GetContainer(_options.DatabaseId, _options.ContainerIds.ElementAt(containerIndex));
            _containers.ElementAt(containerIndex).Value.Received(1)
                .GetItemQueryStreamIterator(Arg.Is<QueryDefinition>(q => q.QueryText.Equals($"SELECT 1 FROM {_options.ContainerIds.ElementAt(containerIndex)}")));
        }

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_database()
    {
        using var tokenSource = new CancellationTokenSource();
        _options.ContainerIds = ContainerIds;
        _options.DatabaseId = DatabaseId;

        _cosmosClient.GetContainer(_options.DatabaseId, _options.ContainerIds.ElementAt(0)).Throws(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_container()
    {
        using var tokenSource = new CancellationTokenSource();
        _options.ContainerIds = ContainerIds;
        _options.DatabaseId = DatabaseId;

        _cosmosClient.GetContainer(_options.DatabaseId, _options.ContainerIds.ElementAt(0)).Returns(_containers[ContainerIds[0]]);
        _containers.ElementAt(0).Value.GetItemQueryStreamIterator(Arg.Any<QueryDefinition>()).Throws(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task return_unhealthy_when_checking_unhealthy_service(bool checkDatabase, bool checkContainers)
    {
        using var tokenSource = new CancellationTokenSource();

        _cosmosClient
            .ReadAccountAsync()
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        _options.ContainerIds = checkContainers ? ContainerIds : null;
        _options.DatabaseId = checkDatabase ? DatabaseId : null;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync();

        await _database
            .DidNotReceiveWithAnyArgs()
            .ReadAsync(default);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)));

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(_cosmosClient)
            .AddLogging()
            .AddHealthChecks()
            .AddAzureCosmosDB(optionsFactory: _ => new AzureCosmosDbHealthCheckOptions() { DatabaseId = DatabaseId }, name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _cosmosClient
            .ReadAccountAsync()
            .Returns(Substitute.For<AccountProperties>());

        _database
            .ReadAsync(null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Database not found"));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync();

        await _database
            .Received(1)
            .ReadAsync(null, Arg.Any<CancellationToken>());

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)));

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception!.ShouldBeOfType<RequestFailedException>();
    }
}
