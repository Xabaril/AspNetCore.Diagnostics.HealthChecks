using System.Net;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.AzureStorage.Tests;

public class azureblobstoragehealthcheck_should
{
    private const string ContainerName = "unit-test";
    private const string HealthCheckName = "unit-test-check";

    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly AzureBlobStorageHealthCheckOptions _options;
    private readonly AzureBlobStorageHealthCheck _healthCheck;
    private readonly HealthCheckContext _context;

    public azureblobstoragehealthcheck_should()
    {
        _blobServiceClient = Substitute.For<BlobServiceClient>();
        _blobContainerClient = Substitute.For<BlobContainerClient>();
        _options = new AzureBlobStorageHealthCheckOptions();
        _healthCheck = new AzureBlobStorageHealthCheck(_blobServiceClient, _options);
        _context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, _healthCheck, HealthStatus.Unhealthy, null)
        };

        _blobServiceClient.GetBlobContainerClient(ContainerName).Returns(_blobContainerClient);
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service()
    {
        using var tokenSource = new CancellationTokenSource();

        _blobServiceClient
            .GetBlobContainersAsync(cancellationToken: tokenSource.Token)
            .Returns(AsyncPageable<BlobContainerItem>.FromPages(Array.Empty<Page<BlobContainerItem>>()));

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _blobServiceClient
            .Received(1)
            .GetBlobContainersAsync(cancellationToken: tokenSource.Token);

        await _blobContainerClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default, default);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_and_container()
    {
        using var tokenSource = new CancellationTokenSource();

        _blobServiceClient
            .GetBlobContainersAsync(cancellationToken: tokenSource.Token)
            .Returns(AsyncPageable<BlobContainerItem>.FromPages(new[] { Substitute.For<Page<BlobContainerItem>>() }));

        _blobContainerClient
            .GetPropertiesAsync(conditions: null, cancellationToken: tokenSource.Token)
            .Returns(Substitute.For<Response<BlobContainerProperties>>());

        _options.ContainerName = ContainerName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _blobServiceClient
            .Received(1)
            .GetBlobContainersAsync(cancellationToken: tokenSource.Token);

        await _blobContainerClient
            .Received(1)
            .GetPropertiesAsync(conditions: null, cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task return_unhealthy_when_checking_unhealthy_service(bool checkContainer)
    {
        using var tokenSource = new CancellationTokenSource();

        var pageable = Substitute.For<AsyncPageable<BlobContainerItem>>();
        var enumerable = Substitute.For<IAsyncEnumerable<Page<BlobContainerItem>>>();
        var enumerator = Substitute.For<IAsyncEnumerator<Page<BlobContainerItem>>>();

        pageable
            .AsPages(pageSizeHint: 1)
            .Returns(enumerable);

        enumerable
            .GetAsyncEnumerator(tokenSource.Token)
            .Returns(enumerator);

        enumerator
            .MoveNextAsync()
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        _blobServiceClient
            .GetBlobContainersAsync(cancellationToken: tokenSource.Token)
            .Returns(pageable);

        _options.ContainerName = checkContainer ? ContainerName : null;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _blobServiceClient
            .Received(1)
            .GetBlobContainersAsync(cancellationToken: tokenSource.Token);

        pageable
            .Received(1)
            .AsPages(pageSizeHint: 1);

        enumerable
            .Received(1)
            .GetAsyncEnumerator(tokenSource.Token);

        await enumerator
            .Received(1)
            .MoveNextAsync();

        await _blobContainerClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default, default);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_container()
    {
        using var tokenSource = new CancellationTokenSource();

        _blobServiceClient
            .GetBlobContainersAsync(cancellationToken: tokenSource.Token)
            .Returns(AsyncPageable<BlobContainerItem>.FromPages(new[] { Substitute.For<Page<BlobContainerItem>>() }));

        _blobContainerClient
            .GetPropertiesAsync(conditions: null, cancellationToken: tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Container not found"));

        _options.ContainerName = ContainerName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _blobServiceClient
            .Received(1)
            .GetBlobContainersAsync(cancellationToken: tokenSource.Token);

        await _blobContainerClient
            .Received(1)
            .GetPropertiesAsync(conditions: null, cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(_blobServiceClient)
            .AddLogging()
            .AddHealthChecks()
            .AddAzureBlobStorage(o => o.ContainerName = ContainerName, name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _blobServiceClient
            .GetBlobContainersAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(AsyncPageable<BlobContainerItem>.FromPages(new[] { Substitute.For<Page<BlobContainerItem>>() }));

        _blobContainerClient
            .GetPropertiesAsync(conditions: null, cancellationToken: Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Container not found"));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        _blobServiceClient
            .Received(1)
            .GetBlobContainersAsync(cancellationToken: Arg.Any<CancellationToken>());

        await _blobContainerClient
            .Received(1)
            .GetPropertiesAsync(conditions: null, cancellationToken: Arg.Any<CancellationToken>());

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception!.ShouldBeOfType<RequestFailedException>();
    }
}
