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
    private readonly BlobStorageHealthCheckOptions _options;
    private readonly AzureBlobStorageHealthCheck _healthCheck;
    private readonly HealthCheckContext _context;

    public azureblobstoragehealthcheck_should()
    {
        var snapshot = Substitute.For<IOptionsSnapshot<BlobStorageHealthCheckOptions>>();

        _blobServiceClient = Substitute.For<BlobServiceClient>();
        _blobContainerClient = Substitute.For<BlobContainerClient>();
        _options = new BlobStorageHealthCheckOptions();
        _healthCheck = new AzureBlobStorageHealthCheck(_blobServiceClient, snapshot);
        _context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, _healthCheck, HealthStatus.Unhealthy, null)
        };

        snapshot.Get(HealthCheckName).Returns(_options);
        _blobServiceClient.GetBlobContainerClient(ContainerName).Returns(_blobContainerClient);
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service()
    {
        using var tokenSource = new CancellationTokenSource();

        _blobServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<BlobServiceProperties>>());

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _blobServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _blobContainerClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default, default);

        actual.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_and_container()
    {
        using var tokenSource = new CancellationTokenSource();

        _blobServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<BlobServiceProperties>>());

        _blobContainerClient
            .GetPropertiesAsync(conditions: null, cancellationToken: tokenSource.Token)
            .Returns(Substitute.For<Response<BlobContainerProperties>>());

        _options.ContainerName = ContainerName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _blobServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _blobContainerClient
            .Received(1)
            .GetPropertiesAsync(conditions: null, cancellationToken: tokenSource.Token);

        actual.Status.Should().Be(HealthStatus.Healthy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task throw_when_checking_unhealthy_service(bool checkContainer)
    {
        using var tokenSource = new CancellationTokenSource();

        _blobServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        _options.ContainerName = checkContainer ? ContainerName : null;
        var actual = await _healthCheck
            .CheckHealthAsync(_context, tokenSource.Token)
            .ShouldThrowAsync<RequestFailedException>();

        await _blobServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _blobContainerClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default, default);

        actual.Status.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task throw_when_checking_unhealthy_container()
    {
        using var tokenSource = new CancellationTokenSource();

        _blobServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<BlobServiceProperties>>());

        _blobContainerClient
            .GetPropertiesAsync(conditions: null, cancellationToken: tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Container not found"));

        _options.ContainerName = ContainerName;
        var actual = await _healthCheck
            .CheckHealthAsync(_context, tokenSource.Token)
            .ShouldThrowAsync<RequestFailedException>();

        await _blobServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _blobContainerClient
            .Received(1)
            .GetPropertiesAsync(conditions: null, cancellationToken: tokenSource.Token);

        actual.Status.Should().Be((int)HttpStatusCode.NotFound);
    }
}
