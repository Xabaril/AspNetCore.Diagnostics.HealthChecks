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
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<QueueServiceProperties>>());

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _queueServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _queueClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_and_queue()
    {
        using var tokenSource = new CancellationTokenSource();

        _queueServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<QueueServiceProperties>>());

        _queueClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<QueueProperties>>());

        _options.QueueName = QueueName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _queueServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _queueClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task return_unhealthy_when_checking_unhealthy_service(bool checkQueue)
    {
        using var tokenSource = new CancellationTokenSource();

        _queueServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        _options.QueueName = checkQueue ? QueueName : null;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _queueServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _queueClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default);

        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_queue()
    {
        using var tokenSource = new CancellationTokenSource();

        _queueServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<QueueServiceProperties>>());

        _queueClient
            .GetPropertiesAsync(tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Queue not found"));

        _options.QueueName = QueueName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _queueServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _queueClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

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
            .AddAzureQueueStorage(o => o.QueueName = QueueName, name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _queueServiceClient
            .GetPropertiesAsync(Arg.Any<CancellationToken>())
            .Returns(Substitute.For<Response<QueueServiceProperties>>());

        _queueClient
            .GetPropertiesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Queue not found"));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        await _queueServiceClient
            .Received(1)
            .GetPropertiesAsync(Arg.Any<CancellationToken>());

        await _queueClient
            .Received(1)
            .GetPropertiesAsync(Arg.Any<CancellationToken>());

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception!.ShouldBeOfType<RequestFailedException>();
    }
}
