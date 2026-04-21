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
    private readonly string OtherQueueName;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusReceiver _serviceBusQueueReceiver;
    private readonly ServiceBusSender _serviceBusQueueSender;
    private readonly ServiceBusReceiver _serviceBusOtherQueueReceiver;
    private readonly ServiceBusSender _serviceBusOtherQueueSender;
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
    private readonly TokenCredential _tokenCredential;

    public azureservicebusqueuehealthcheck_should()
    {
        ConnectionString = Guid.NewGuid().ToString();
        FullyQualifiedName = Guid.NewGuid().ToString();
        QueueName = Guid.NewGuid().ToString();
        OtherQueueName = Guid.NewGuid().ToString();

        _serviceBusClient = Substitute.For<ServiceBusClient>();
        _serviceBusQueueReceiver = Substitute.For<ServiceBusReceiver>();
        _serviceBusQueueSender = Substitute.For<ServiceBusSender>();
        _serviceBusOtherQueueReceiver = Substitute.For<ServiceBusReceiver>();
        _serviceBusOtherQueueSender = Substitute.For<ServiceBusSender>();
        _clientProvider = Substitute.For<ServiceBusClientProvider>();
        _serviceBusAdministrationClient = Substitute.For<ServiceBusAdministrationClient>();
        _tokenCredential = Substitute.For<TokenCredential>();

        _clientProvider.CreateClient(ConnectionString).Returns(_serviceBusClient);
        _clientProvider.CreateClient(FullyQualifiedName, _tokenCredential).Returns(_serviceBusClient);
        _clientProvider.CreateManagementClient(ConnectionString).Returns(_serviceBusAdministrationClient);
        _clientProvider.CreateManagementClient(FullyQualifiedName, _tokenCredential).Returns(_serviceBusAdministrationClient);
        _serviceBusClient.CreateReceiver(QueueName).Returns(_serviceBusQueueReceiver);
        _serviceBusClient.CreateSender(QueueName).Returns(_serviceBusQueueSender);
        _serviceBusClient.CreateReceiver(OtherQueueName).Returns(_serviceBusOtherQueueReceiver);
        _serviceBusClient.CreateSender(OtherQueueName).Returns(_serviceBusOtherQueueSender);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task can_create_client_with_connection_string(bool peakMode, bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            createMessageBatchAsyncMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        if (peakMode || createMessageBatchAsyncMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(ConnectionString);
        }
        else if (peakMode && createMessageBatchAsyncMode)
        {
            _clientProvider
                .Received(2)
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
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task reuses_existing_client_when_using_same_connection_string_with_different_queue(bool peakMode, bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            createMessageBatchAsyncMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        await ExecuteHealthCheckAsync(
            OtherQueueName,
            peakMode,
            createMessageBatchAsyncMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        if (peakMode || createMessageBatchAsyncMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(ConnectionString);

            if (peakMode)
            {
                _serviceBusClient
                    .Received(1)
                    .CreateReceiver(QueueName);

                _serviceBusClient
                    .Received(1)
                    .CreateReceiver(OtherQueueName);

                await _serviceBusQueueReceiver
                    .Received(1)
                    .PeekMessageAsync(cancellationToken: tokenSource.Token);

                await _serviceBusOtherQueueReceiver
                    .Received(1)
                    .PeekMessageAsync(cancellationToken: tokenSource.Token);
            }
            else if (createMessageBatchAsyncMode)
            {
                _serviceBusClient
                    .Received(1)
                    .CreateSender(QueueName);

                _serviceBusClient
                    .Received(1)
                    .CreateSender(OtherQueueName);

                await _serviceBusQueueSender
                    .Received(1)
                    .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);

                await _serviceBusOtherQueueSender
                    .Received(1)
                    .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);
            }
        }
        else
        {
            _clientProvider
                .Received(1)
                .CreateManagementClient(ConnectionString);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetQueueRuntimePropertiesAsync(QueueName, tokenSource.Token);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetQueueRuntimePropertiesAsync(OtherQueueName, tokenSource.Token);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task can_create_client_with_fully_qualified_endpoint(bool peakMode, bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            createMessageBatchAsyncMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        if (peakMode || createMessageBatchAsyncMode)
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
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task reuses_existing_client_when_using_same_fully_qualified_name_with_different_queue(bool peakMode, bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            createMessageBatchAsyncMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        await ExecuteHealthCheckAsync(
            OtherQueueName,
            peakMode,
            createMessageBatchAsyncMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        if (peakMode || createMessageBatchAsyncMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(FullyQualifiedName, _tokenCredential);

            if (peakMode)
            {
                _serviceBusClient
                    .Received(1)
                    .CreateReceiver(QueueName);

                _serviceBusClient
                    .Received(1)
                    .CreateReceiver(OtherQueueName);

                await _serviceBusQueueReceiver
                    .Received(1)
                    .PeekMessageAsync(cancellationToken: tokenSource.Token);

                await _serviceBusOtherQueueReceiver
                    .Received(1)
                    .PeekMessageAsync(cancellationToken: tokenSource.Token);
            }
            else if (createMessageBatchAsyncMode)
            {
                _serviceBusClient
                    .Received(1)
                    .CreateSender(QueueName);

                _serviceBusClient
                    .Received(1)
                    .CreateSender(OtherQueueName);

                await _serviceBusQueueSender
                    .Received(1)
                    .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);

                await _serviceBusOtherQueueSender
                    .Received(1)
                    .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);

            }
        }
        else
        {
            _clientProvider
                .Received(1)
                .CreateManagementClient(FullyQualifiedName, _tokenCredential);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetQueueRuntimePropertiesAsync(QueueName, tokenSource.Token);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetQueueRuntimePropertiesAsync(OtherQueueName, tokenSource.Token);
        }
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_peek_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            true,
            false,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _serviceBusClient
            .Received(1)
            .CreateReceiver(QueueName);

        await _serviceBusQueueReceiver
            .Received(1)
            .PeekMessageAsync(cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_createmessagebatch_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            false,
            true,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _serviceBusClient
            .Received(1)
            .CreateSender(QueueName);

        await _serviceBusQueueSender
            .Received(1)
            .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_peek_and_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            true,
            false,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _serviceBusClient
            .Received(1)
            .CreateReceiver(QueueName);

        await _serviceBusQueueReceiver
            .Received(1)
            .PeekMessageAsync(cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_createmessagebatch_and_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            false,
            true,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _serviceBusClient
            .Received(1)
            .CreateSender(QueueName);

        await _serviceBusQueueSender
            .Received(1)
            .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task return_unhealthy_when_exception_is_thrown_by_client(bool peakMode, bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();

        _serviceBusQueueReceiver
            .PeekMessageAsync(cancellationToken: tokenSource.Token)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        _serviceBusQueueSender
            .CreateMessageBatchAsync(cancellationToken: tokenSource.Token)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            peakMode,
            createMessageBatchAsyncMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);

        if (peakMode || createMessageBatchAsyncMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(ConnectionString);
        }

        if (peakMode)
        {
            _serviceBusClient
                .ReceivedCalls().Count(call => call.GetMethodInfo().Name == "CreateReceiver" && call.GetArguments()[0]?.Equals(QueueName) == true)
                .ShouldBeLessThanOrEqualTo(1);

            _serviceBusQueueReceiver
                .ReceivedCalls().Count(call => call.GetMethodInfo().Name == "PeekMessageAsync")
                .ShouldBeLessThanOrEqualTo(1);
        }

        if (createMessageBatchAsyncMode)
        {
            _serviceBusClient
                .ReceivedCalls().Count(call => call.GetMethodInfo().Name == "CreateSender" && call.GetArguments()[0]?.Equals(QueueName) == true)
                .ShouldBeLessThanOrEqualTo(1);

            _serviceBusQueueSender
                .ReceivedCalls().Count(call => call.GetMethodInfo().Name == "CreateMessageBatchAsync")
                .ShouldBeLessThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_administration_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            false,
            false,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_administration_and_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            QueueName,
            false,
            false,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token);
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
            false,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);

        await _serviceBusAdministrationClient
           .Received(1)
           .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token);
    }

    private Task<HealthCheckResult> ExecuteHealthCheckAsync(
        string queueName,
        bool peakMode,
        bool createMessageBatchAsyncMode,
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
            UseCreateMessageBatchAsyncMode = createMessageBatchAsyncMode
        };
        var healthCheck = new AzureServiceBusQueueHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };

        return healthCheck.CheckHealthAsync(context, cancellationToken);
    }
}
