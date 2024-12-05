using System.Net;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.Azure.Data.Tables.Tests;

public class tableservicehealthcheck_should
{
    private const string TableName = "unit-test";
    private const string HealthCheckName = "unit-test-check";

    private readonly TableServiceClient _tableServiceClient;
    private readonly TableClient _tableClient;
    private readonly AzureTableServiceHealthCheckOptions _options;
    private readonly AzureTableServiceHealthCheck _healthCheck;
    private readonly HealthCheckContext _context;

    public tableservicehealthcheck_should()
    {
        _tableServiceClient = Substitute.For<TableServiceClient>();
        _tableClient = Substitute.For<TableClient>();
        _options = new AzureTableServiceHealthCheckOptions();
        _healthCheck = new AzureTableServiceHealthCheck(_tableServiceClient, _options);
        _context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, _healthCheck, HealthStatus.Unhealthy, null)
        };

        _tableServiceClient.GetTableClient(TableName).Returns(_tableClient);
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service()
    {
        using var tokenSource = new CancellationTokenSource();

        _tableServiceClient
            .QueryAsync(filter: "false", cancellationToken: tokenSource.Token)
            .Returns(AsyncPageable<TableItem>.FromPages([]));

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _tableServiceClient
            .Received(1)
            .QueryAsync(filter: "false", cancellationToken: tokenSource.Token);

        _tableClient
            .DidNotReceiveWithAnyArgs()
            .QueryAsync<TableEntity>(default(string), default, default, default);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_table()
    {
        using var tokenSource = new CancellationTokenSource();

        _tableClient
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: tokenSource.Token)
            .Returns(AsyncPageable<TableEntity>.FromPages([]));

        _options.TableName = TableName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _tableClient
            .Received(1)
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_service()
    {
        using var tokenSource = new CancellationTokenSource();

        var pageable = Substitute.For<AsyncPageable<TableItem>>();
        var enumerator = Substitute.For<IAsyncEnumerator<TableItem>>();

        _tableServiceClient
            .QueryAsync(filter: "false", cancellationToken: tokenSource.Token)
            .Returns(pageable);

        pageable
            .GetAsyncEnumerator(tokenSource.Token)
            .Returns(enumerator);

        enumerator
            .MoveNextAsync()
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));

        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _tableServiceClient
            .Received(1)
            .QueryAsync(filter: "false", cancellationToken: tokenSource.Token);

        pageable
            .Received(1)
            .GetAsyncEnumerator(tokenSource.Token);

        await enumerator
            .Received(1)
            .MoveNextAsync();

        _tableClient
            .DidNotReceiveWithAnyArgs()
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_service_queue()
    {
        using var tokenSource = new CancellationTokenSource();

        _tableClient
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: tokenSource.Token)
            .Throws(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Unable to authorize access."));


        _options.TableName = TableName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _tableClient
            .Received(1)
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task return_unhealthy_when_checking_unhealthy_table()
    {
        using var tokenSource = new CancellationTokenSource();

        _tableClient
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: tokenSource.Token)
            .Throws(new RequestFailedException((int)HttpStatusCode.NotFound, "Table not found"));

        _options.TableName = TableName;
        var actual = await _healthCheck.CheckHealthAsync(_context, tokenSource.Token);

        _tableClient
            .Received(1)
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: tokenSource.Token);


        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual
            .Exception!.ShouldBeOfType<RequestFailedException>()
            .Status.ShouldBe((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(_tableServiceClient)
            .AddLogging()
            .AddHealthChecks()
            .AddAzureTable(optionsFactory: _ => new AzureTableServiceHealthCheckOptions(), name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        var pageable = Substitute.For<AsyncPageable<TableItem>>();
        var enumerator = Substitute.For<IAsyncEnumerator<TableItem>>();

        _tableServiceClient
            .QueryAsync(filter: "false", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(pageable);

        pageable
            .GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(enumerator);

        enumerator
            .MoveNextAsync()
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Table not found"));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        _tableServiceClient
            .Received(1)
            .QueryAsync(filter: "false", cancellationToken: Arg.Any<CancellationToken>());

        pageable
            .Received(1)
            .GetAsyncEnumerator(Arg.Any<CancellationToken>());

        await enumerator
            .Received(1)
            .MoveNextAsync();

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception!.ShouldBeOfType<RequestFailedException>();
    }


    [Fact]
    public async Task return_unhealthy_when_invoked_from_healthcheckservice_for_table()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(_tableServiceClient)
            .AddLogging()
            .AddHealthChecks()
            .AddAzureTable(optionsFactory: _ => new AzureTableServiceHealthCheckOptions() { TableName = TableName }, name: HealthCheckName)
            .Services
            .BuildServiceProvider();

        _tableClient
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: Arg.Any<CancellationToken>())
            .Throws(new RequestFailedException((int)HttpStatusCode.NotFound, "Table not found"));

        var service = provider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync();

        _tableClient
            .Received(1)
            .QueryAsync<TableEntity>(filter: "false", cancellationToken: Arg.Any<CancellationToken>());

        var actual = report.Entries[HealthCheckName];
        actual.Status.ShouldBe(HealthStatus.Unhealthy);
        actual.Exception!.ShouldBeOfType<RequestFailedException>();
    }
}
