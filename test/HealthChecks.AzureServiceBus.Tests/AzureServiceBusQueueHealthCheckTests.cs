using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.AzureServiceBus.Tests;

public class azureservicebusqueuehealthcheck_should
{
    private const string HEALTH_CHECK_NAME = "unit-test-check";

    private readonly string ConnectionString;
    private readonly string FullyQualifiedName;
    private readonly string QueueName;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusReceiver _serviceBusReceiver;
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
    private readonly TokenCredential _tokenCredential;

    public azureservicebusqueuehealthcheck_should()
    {
        ConnectionString = Guid.NewGuid().ToString();
        FullyQualifiedName = Guid.NewGuid().ToString();
        QueueName = Guid.NewGuid().ToString();

        _serviceBusClient = Substitute.For<ServiceBusClient>();
        _serviceBusReceiver = Substitute.For<ServiceBusReceiver>();
        _clientProvider = Substitute.For<ServiceBusClientProvider>();
        _serviceBusAdministrationClient = Substitute.For<ServiceBusAdministrationClient>();
        _tokenCredential = Substitute.For<TokenCredential>();

        _clientProvider.CreateClient(ConnectionString).Returns(_serviceBusClient);
        _clientProvider.CreateClient(FullyQualifiedName, _tokenCredential).Returns(_serviceBusClient);
        _clientProvider.CreateManagementClient(ConnectionString).Returns(_serviceBusAdministrationClient);
        _clientProvider.CreateManagementClient(FullyQualifiedName, _tokenCredential).Returns(_serviceBusAdministrationClient);
        _serviceBusClient.CreateReceiver(QueueName).Returns(_serviceBusReceiver);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task can_create_client_with_connection_string(bool peakMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        if (peakMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(ConnectionString);
        }
        else
        {
            _clientProvider
                .Received(1)
                .CreateManagementClient(ConnectionString);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task reuses_existing_client_when_using_same_connection_string_with_different_queue(bool peakMode)
    {
        using var tokenSource = new CancellationTokenSource();
        var otherQueueName = Guid.NewGuid().ToString();

        await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token).ConfigureAwait(false);

        await ExecuteHealthCheckAsync(
            otherQueueName,
            peakMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        if (peakMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(ConnectionString);

            _serviceBusClient
                .Received(1)
                .CreateReceiver(QueueName);

            _serviceBusClient
                .Received(1)
                .CreateReceiver(otherQueueName);
        }
        else
        {
            _clientProvider
                .Received(1)
                .CreateManagementClient(ConnectionString);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetQueueRuntimePropertiesAsync(QueueName, tokenSource.Token)
                .ConfigureAwait(false);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetQueueRuntimePropertiesAsync(otherQueueName, tokenSource.Token)
                .ConfigureAwait(false);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task can_create_client_with_fully_qualified_endpoint(bool peakMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        if (peakMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(FullyQualifiedName, _tokenCredential);
        }
        else
        {
            _clientProvider
                .Received(1)
                .CreateManagementClient(FullyQualifiedName, _tokenCredential);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task reuses_existing_client_when_using_same_fully_qualified_name_with_different_queue(bool peakMode)
    {
        using var tokenSource = new CancellationTokenSource();
        var otherQueueName = Guid.NewGuid().ToString();

        await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        await ExecuteHealthCheckAsync(
            otherQueueName,
            peakMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        if (peakMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(FullyQualifiedName, _tokenCredential);

            _serviceBusClient
                .Received(1)
                .CreateReceiver(QueueName);

            _serviceBusClient
                .Received(1)
                .CreateReceiver(otherQueueName);
        }
        else
        {
            _clientProvider
                .Received(1)
                .CreateManagementClient(FullyQualifiedName, _tokenCredential);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetQueueRuntimePropertiesAsync(QueueName, tokenSource.Token)
                .ConfigureAwait(false);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetQueueRuntimePropertiesAsync(otherQueueName, tokenSource.Token)
                .ConfigureAwait(false);
        }
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_peek_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            true,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _serviceBusClient
            .Received(1)
            .CreateReceiver(QueueName);

        await _serviceBusReceiver
            .Received(1)
            .PeekMessageAsync(cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_peek_and_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            true,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _serviceBusClient
            .Received(1)
            .CreateReceiver(QueueName);

        await _serviceBusReceiver
            .Received(1)
            .PeekMessageAsync(cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task return_unhealthy_when_exception_is_thrown_by_client()
    {
        using var tokenSource = new CancellationTokenSource();

        _serviceBusReceiver
            .PeekMessageAsync(cancellationToken: tokenSource.Token)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            true,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);

        _serviceBusClient
            .Received(1)
            .CreateReceiver(QueueName);

        await _serviceBusReceiver
            .Received(1)
            .PeekMessageAsync(cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_administration_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            false,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_administration_and_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            false,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task return_unhealthy_when_exception_is_thrown_by_administration_client()
    {
        using var tokenSource = new CancellationTokenSource();

        _serviceBusAdministrationClient
            .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            false,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);

        await _serviceBusAdministrationClient
           .Received(1)
           .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token)
           .ConfigureAwait(false);
    }

    private Task<HealthCheckResult> ExecuteHealthCheckAsync(
        string queueName,
        bool peakMode,
        string? connectionString = null,
        string? fullyQualifiedName = null,
        CancellationToken cancellationToken = default)
    {
        var options = new AzureServiceBusQueueHealthCheckOptions(queueName)
        {
            ConnectionString = connectionString,
            FullyQualifiedNamespace = fullyQualifiedName,
            Credential = fullyQualifiedName is null ? null : _tokenCredential,
            UsePeekMode = peakMode,
        };
        var healthCheck = new AzureServiceBusQueueHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };

        return healthCheck.CheckHealthAsync(context, cancellationToken);
    }
}
