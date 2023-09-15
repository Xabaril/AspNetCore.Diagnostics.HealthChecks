using System.Net;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.AzureStorage.Tests;

public class azurequeuestoragehealthcheck_should
{
    private const string QueueName = "unit-test";
    private const string HealthCheckName = "unit-test-check";

    private readonly QueueServiceClient _queueServiceClient;
    private readonly QueueClient _queueClient;
    private readonly AzureQueueStorageHealthCheckOptions _options;
    private readonly AzureQueueStorageHealthCheck _healthCheck;
    private readonly HealthCheckContext _context;

    public azurequeuestoragehealthcheck_should()
    {
        _queueServiceClient = Substitute.For<QueueServiceClient>();
        _queueClient = Substitute.For<QueueClient>();
        _options = new AzureQueueStorageHealthCheckOptions();
        _healthCheck = new AzureQueueStorageHealthCheck(_queueServiceClient, _options);
        _context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("unit-test-check", _healthCheck, HealthStatus.Unhealthy, null)
        };

        _queueServiceClient.GetQueueClient(QueueName).Returns(_queueClient);
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service()
    {
        using var tokenSource = new CancellationTokenSource();

        _queueServiceClient
            .GetQueuesAsync(cancellationToken: tokenSource.Token)
            .Returns(AsyncPageable<QueueItem>.FromPages(Array.Empty<Page<QueueItem>>()));

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        _queueServiceClient
            .Received(1)
            .GetQueuesAsync(cancellationToken: tokenSource.Token);

        await _queueClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_and_queue()
    {
        using var tokenSource = new CancellationTokenSource();

        _queueServiceClient
            .GetQueuesAsync(cancellationToken: tokenSource.Token)
            .Returns(AsyncPageable<QueueItem>.FromPages(new[] { Substitute.For<Page<QueueItem>>() }));

        _queueClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<QueueProperties>>());

        _options.QueueName = QueueName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        _queueServiceClient
            .Received(1)
            .GetQueuesAsync(cancellationToken: tokenSource.Token);

        await _queueClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task return_unhealthy_when_checking_unhealthy_service(bool checkQueue)
    {
        using var tokenSource = new CancellationTokenSource();

        var pageable = Substitute.For<AsyncPageable<QueueItem>>();
        var enumerable = Substitute.For<IAsyncEnumerable<Page<QueueItem>>>();
        var enumerator = Substitute.For<IAsyncEnumerator<Page<QueueItem>>>();

        pageable
            .AsPages(pageSizeHint: 1)
            .Returns(enumerable);

        enumerable
            .GetAsyncEnumerator(tokenSource.Token)
            .Returns(enumerator);

        enumerator
            .MoveNextAsync()
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        _queueServiceClient
            .GetQueuesAsync(cancellationToken: tokenSource.Token)
            .Returns(pageable);

        _options.QueueName = checkQueue ? QueueName : null;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        _queueServiceClient
            .Received(1)
            .GetQueuesAsync(cancellationToken: tokenSource.Token);

        pageable
            .Received(1)
            .AsPages(pageSizeHint: 1);

        enumerable
            .Received(1)
            .GetAsyncEnumerator(tokenSource.Token);

        await enumerator
            .Received(1)
            .MoveNextAsync()
            .ConfigureAwait(false);

        await _queueClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default)
            .ConfigureAwait(false);

        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_queue()
    {
        using var tokenSource = new CancellationTokenSource();

        _queueServiceClient
            .GetQueuesAsync(cancellationToken: tokenSource.Token)
            .Returns(AsyncPageable<QueueItem>.FromPages(new[] { Substitute.For<Page<QueueItem>>() }));

        _queueClient
            .GetPropertiesAsync(tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Queue not found"));

        _options.QueueName = QueueName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token).ConfigureAwait(false);

        _queueServiceClient
            .Received(1)
            .GetQueuesAsync(cancellationToken: tokenSource.Token);

        await _queueClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token)
            .ConfigureAwait(false);

        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(_queueServiceClient)
            .AddLogging()
            .AddHealthChecks()
            .AddAzureQueueStorage(optionsFactory: _ => new AzureQueueStorageHealthCheckOptions() { QueueName = QueueName }, healthCheckName: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _queueServiceClient
            .GetQueuesAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(AsyncPageable<QueueItem>.FromPages(new[] { Substitute.For<Page<QueueItem>>() }));

        _queueClient
            .GetPropertiesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Queue not found"));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync().ConfigureAwait(false);

        _queueServiceClient
            .Received(1)
            .GetQueuesAsync(cancellationToken: Arg.Any<CancellationToken>());

        await _queueClient
            .Received(1)
            .GetPropertiesAsync(Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception!.ShouldBeOfType<RequestFailedException>();
    }
}
