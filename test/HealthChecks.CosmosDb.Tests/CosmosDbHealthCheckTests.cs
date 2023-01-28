using System.Net;
using Azure;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.CosmosDb.Tests;

public class cosmosdbhealthcheck_should
{
    private static readonly string[] ContainerIds = { "one", "two", "three" };
    private const string DatabaseId = "unit-test";
    private const string HealthCheckName = "unit-test-check";

    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly IReadOnlyDictionary<string, Container> _containers;
    private readonly CosmosDbHealthCheckOptions _options;
    private readonly CosmosDbHealthCheck _healthCheck;
    private readonly HealthCheckContext _context;

    public cosmosdbhealthcheck_should()
    {
        _cosmosClient = Substitute.For<CosmosClient>();
        _database = Substitute.For<Database>();
        _options = new CosmosDbHealthCheckOptions();
        _healthCheck = new CosmosDbHealthCheck(_cosmosClient, _options);
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

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync()
            .ConfigureAwait(false);

        await _database
            .DidNotReceiveWithAnyArgs()
            .ReadAsync(default)
            .ConfigureAwait(false);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)))
                .ConfigureAwait(false);

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
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync()
            .ConfigureAwait(false);

        await _database
            .Received(1)
            .ReadAsync(null, tokenSource.Token)
            .ConfigureAwait(false);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)))
                .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_database_and_containers()
    {
        using var tokenSource = new CancellationTokenSource();

        _cosmosClient
            .ReadAccountAsync()
            .Returns(Substitute.For<AccountProperties>());

        _database
            .ReadAsync(null, tokenSource.Token)
            .Returns(Substitute.For<DatabaseResponse>());

        foreach (var container in _containers.Values)
        {
            container
                .ReadContainerAsync(null, tokenSource.Token)
                .Returns(Substitute.For<ContainerResponse>());
        }

        _options.ContainerIds = ContainerIds;
        _options.DatabaseId = DatabaseId;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync()
            .ConfigureAwait(false);

        await _database
            .Received(1)
            .ReadAsync(null, tokenSource.Token)
            .ConfigureAwait(false);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .Received(1)
                .ReadContainerAsync(null, tokenSource.Token)))
                .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);
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
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync()
            .ConfigureAwait(false);

        await _database
            .DidNotReceiveWithAnyArgs()
            .ReadAsync(default)
            .ConfigureAwait(false);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)))
                .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task return_unhealthy_when_checking_unhealthy_database(bool checkContainers)
    {
        using var tokenSource = new CancellationTokenSource();

        _cosmosClient
            .ReadAccountAsync()
            .Returns(Substitute.For<AccountProperties>());

        _database
            .ReadAsync(null, tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Database not found"));

        _options.ContainerIds = checkContainers ? ContainerIds : null;
        _options.DatabaseId = DatabaseId;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync()
            .ConfigureAwait(false);

        await _database
            .Received(1)
            .ReadAsync(null, tokenSource.Token)
            .ConfigureAwait(false);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)))
                .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_container()
    {
        using var tokenSource = new CancellationTokenSource();

        _cosmosClient
            .ReadAccountAsync()
            .Returns(Substitute.For<AccountProperties>());

        _database
            .ReadAsync(null, tokenSource.Token)
            .Returns(Substitute.For<DatabaseResponse>());

        _containers["one"]
            .ReadContainerAsync(null, tokenSource.Token)
            .Returns(Substitute.For<ContainerResponse>());

        _containers["two"]
            .ReadContainerAsync(null, tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Container not found"));

        _options.ContainerIds = ContainerIds;
        _options.DatabaseId = DatabaseId;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync()
            .ConfigureAwait(false);

        await _database
            .Received(1)
            .ReadAsync(null, tokenSource.Token)
            .ConfigureAwait(false);

        await _containers["one"]
            .Received(1)
            .ReadContainerAsync(null, tokenSource.Token)
            .ConfigureAwait(false);

        await _containers["two"]
            .Received(1)
            .ReadContainerAsync(null, tokenSource.Token)
            .ConfigureAwait(false);

        await _containers["three"]
            .DidNotReceiveWithAnyArgs()
            .ReadContainerAsync(default, default)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(_cosmosClient)
            .AddLogging()
            .AddHealthChecks()
            .AddCosmosDb(o => o.DatabaseId = DatabaseId, name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _cosmosClient
            .ReadAccountAsync()
            .Returns(Substitute.For<AccountProperties>());

        _database
            .ReadAsync(null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Database not found"));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync().ConfigureAwait(false);

        await _cosmosClient
            .Received(1)
            .ReadAccountAsync()
            .ConfigureAwait(false);

        await _database
            .Received(1)
            .ReadAsync(null, Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await Task.WhenAll(_containers
            .Values
            .Select(x => x
                .DidNotReceiveWithAnyArgs()
                .ReadContainerAsync(default, default)))
                .ConfigureAwait(false);

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception!.ShouldBeOfType<RequestFailedException>();
    }
}
