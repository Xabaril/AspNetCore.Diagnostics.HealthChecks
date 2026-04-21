using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.AzureServiceBus.Tests;

public class azureservicebustopichealthcheck_should
{
    private const string HEALTH_CHECK_NAME = "unit-test-check";

    private readonly string ConnectionString;
    private readonly string FullyQualifiedName;
    private readonly string TopicName;
    private readonly string OtherTopicName;
    private readonly ServiceBusSender _serviceBusTopicSender;
    private readonly ServiceBusSender _serviceBusOtherTopicSender;
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
    private readonly TokenCredential _tokenCredential;

    public azureservicebustopichealthcheck_should()
    {
        ConnectionString = Guid.NewGuid().ToString();
        FullyQualifiedName = Guid.NewGuid().ToString();
        TopicName = Guid.NewGuid().ToString();
        OtherTopicName = Guid.NewGuid().ToString();

        _serviceBusClient = Substitute.For<ServiceBusClient>();
        _clientProvider = Substitute.For<ServiceBusClientProvider>();
        _serviceBusTopicSender = Substitute.For<ServiceBusSender>();
        _serviceBusOtherTopicSender = Substitute.For<ServiceBusSender>();
        _serviceBusAdministrationClient = Substitute.For<ServiceBusAdministrationClient>();
        _tokenCredential = Substitute.For<TokenCredential>();


        _clientProvider.CreateClient(ConnectionString).Returns(_serviceBusClient);
        _clientProvider.CreateClient(FullyQualifiedName, _tokenCredential).Returns(_serviceBusClient);
        _clientProvider.CreateManagementClient(ConnectionString).Returns(_serviceBusAdministrationClient);
        _clientProvider.CreateManagementClient(FullyQualifiedName, _tokenCredential).Returns(_serviceBusAdministrationClient);
        _serviceBusClient.CreateSender(TopicName).Returns(_serviceBusTopicSender);
        _serviceBusClient.CreateSender(OtherTopicName).Returns(_serviceBusOtherTopicSender);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task can_create_client_with_connection_string(bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            TopicName,
            createMessageBatchAsyncMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        if (createMessageBatchAsyncMode)
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
    public async Task reuses_existing_client_when_using_same_connection_string_with_different_topic(bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();


        await ExecuteHealthCheckAsync(
            TopicName,
            createMessageBatchAsyncMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        await ExecuteHealthCheckAsync(
            OtherTopicName,
            createMessageBatchAsyncMode,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        if (createMessageBatchAsyncMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(ConnectionString);

            _serviceBusClient
                .Received(1)
                .CreateSender(TopicName);

            _serviceBusClient
                .Received(1)
                .CreateSender(OtherTopicName);

            await _serviceBusTopicSender
                .Received(1)
                .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);

            await _serviceBusTopicSender
                .Received(1)
                .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);
        }
        else
        {
            _clientProvider
                .Received(1)
                .CreateManagementClient(ConnectionString);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetTopicRuntimePropertiesAsync(TopicName, tokenSource.Token);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetTopicRuntimePropertiesAsync(OtherTopicName, tokenSource.Token);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task can_create_client_with_fully_qualified_name(bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            TopicName,
            createMessageBatchAsyncMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);


        if (createMessageBatchAsyncMode)
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
    public async Task reuses_existing_client_when_using_same_fully_qualified_name_with_different_topic(bool createMessageBatchAsyncMode)
    {
        using var tokenSource = new CancellationTokenSource();

        await ExecuteHealthCheckAsync(
            TopicName,
            createMessageBatchAsyncMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        await ExecuteHealthCheckAsync(
            OtherTopicName,
            createMessageBatchAsyncMode,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        if (createMessageBatchAsyncMode)
        {
            _clientProvider
                .Received(1)
                .CreateClient(FullyQualifiedName, _tokenCredential);

            _serviceBusClient
                .Received(1)
                .CreateSender(TopicName);

            _serviceBusClient
                .Received(1)
                .CreateSender(OtherTopicName);

            await _serviceBusTopicSender
                .Received(1)
                .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);

            await _serviceBusTopicSender
                .Received(1)
                .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);
        }
        else
        {
            _clientProvider
                .Received(1)
                .CreateManagementClient(FullyQualifiedName, _tokenCredential);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetTopicRuntimePropertiesAsync(TopicName, tokenSource.Token);

            await _serviceBusAdministrationClient
                .Received(1)
                .GetTopicRuntimePropertiesAsync(OtherTopicName, tokenSource.Token);
        }
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_administration_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            TopicName,
            false,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _clientProvider
            .Received(1)
            .CreateManagementClient(ConnectionString);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_administration_and_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            TopicName,
            false,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _clientProvider
            .Received(1)
            .CreateManagementClient(FullyQualifiedName, _tokenCredential);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_createmessagebatch_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            TopicName,
            true,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _serviceBusClient
            .Received(1)
            .CreateSender(TopicName);

        await _serviceBusTopicSender
            .Received(1)
            .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_healthy_when_checking_healthy_service_through_createmessagebatch_and_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();

        var actual = await ExecuteHealthCheckAsync(
            TopicName,
            true,
            fullyQualifiedName: FullyQualifiedName,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _serviceBusClient
            .Received(1)
            .CreateSender(TopicName);

        await _serviceBusTopicSender
            .Received(1)
            .CreateMessageBatchAsync(cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_unhealthy_when_exception_is_thrown_by_administration_client()
    {
        using var tokenSource = new CancellationTokenSource();

        _serviceBusAdministrationClient
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        var actual = await ExecuteHealthCheckAsync(
            TopicName,
            false,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);

        await _serviceBusAdministrationClient
           .Received(1)
           .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token);
    }

    [Fact]
    public async Task return_unhealthy_when_exception_is_thrown_by_client()
    {
        using var tokenSource = new CancellationTokenSource();

        _serviceBusTopicSender
            .CreateMessageBatchAsync(cancellationToken: tokenSource.Token)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        var actual = await ExecuteHealthCheckAsync(
            TopicName,
            true,
            connectionString: ConnectionString,
            cancellationToken: tokenSource.Token);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);

        _clientProvider
            .Received(1)
            .CreateClient(ConnectionString);


        _serviceBusClient
            .Received(1)
            .CreateSender(TopicName);

        await _serviceBusTopicSender
            .Received(1)
            .CreateMessageBatchAsync(tokenSource.Token);

    }

    private Task<HealthCheckResult> ExecuteHealthCheckAsync(
        string topicName,
        bool createMessageBatchAsyncMode,
        string? connectionString = null,
        string? fullyQualifiedName = null,
        CancellationToken cancellationToken = default)
    {
        var options = new AzureServiceBusTopicHealthCheckOptions(topicName)
        {
            ConnectionString = connectionString,
            FullyQualifiedNamespace = fullyQualifiedName,
            Credential = fullyQualifiedName is null ? null : _tokenCredential,
            UseCreateMessageBatchAsyncMode = createMessageBatchAsyncMode
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };

        return healthCheck.CheckHealthAsync(context, cancellationToken);
    }
}
