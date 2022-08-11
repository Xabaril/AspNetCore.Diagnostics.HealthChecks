using System.Net;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.AzureStorage.Tests;

public class azurefilesharehealthcheck_should
{
    private const string ShareName = "unit-test";
    private const string HealthCheckName = "unit-test-check";

    private readonly ShareServiceClient _shareServiceClient;
    private readonly ShareClient _shareClient;
    private readonly AzureFileShareHealthCheckOptions _options;
    private readonly AzureFileShareHealthCheck _healthCheck;
    private readonly HealthCheckContext _context;

    public azurefilesharehealthcheck_should()
    {
        _shareServiceClient = Substitute.For<ShareServiceClient>();
        _shareClient = Substitute.For<ShareClient>();
        _options = new AzureFileShareHealthCheckOptions();
        _healthCheck = new AzureFileShareHealthCheck(_shareServiceClient, _options);
        _context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, _healthCheck, HealthStatus.Unhealthy, null)
        };

        _shareServiceClient.GetShareClient(ShareName).Returns(_shareClient);
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service()
    {
        using var tokenSource = new CancellationTokenSource();

        _shareServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<ShareServiceProperties>>());

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _shareServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _shareClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_and_share()
    {
        using var tokenSource = new CancellationTokenSource();

        _shareServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<ShareServiceProperties>>());

        _shareClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<ShareProperties>>());

        _options.ShareName = ShareName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _shareServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _shareClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task return_unhealthy_when_checking_unhealthy_service(bool checkShare)
    {
        using var tokenSource = new CancellationTokenSource();

        _shareServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        _options.ShareName = checkShare ? ShareName : null;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _shareServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _shareClient
            .DidNotReceiveWithAnyArgs()
            .GetPropertiesAsync(default);

        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_share()
    {
        using var tokenSource = new CancellationTokenSource();

        _shareServiceClient
            .GetPropertiesAsync(tokenSource.Token)
            .Returns(Substitute.For<Response<ShareServiceProperties>>());

        _shareClient
            .GetPropertiesAsync(tokenSource.Token)
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "File share not found"));

        _options.ShareName = ShareName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        await _shareServiceClient
            .Received(1)
            .GetPropertiesAsync(tokenSource.Token);

        await _shareClient
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
            .AddSingleton(_shareServiceClient)
            .AddLogging()
            .AddHealthChecks()
            .AddAzureFileShare((sp, o) => o.ShareName = ShareName, name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _shareServiceClient
            .GetPropertiesAsync(Arg.Any<CancellationToken>())
            .Returns(Substitute.For<Response<ShareServiceProperties>>());

        _shareClient
            .GetPropertiesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "File share not found"));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        await _shareServiceClient
            .Received(1)
            .GetPropertiesAsync(Arg.Any<CancellationToken>());

        await _shareClient
            .Received(1)
            .GetPropertiesAsync(Arg.Any<CancellationToken>());

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception!.ShouldBeOfType<RequestFailedException>();
    }
}
