using Azure;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;
using NSubstitute;

namespace HealthChecks.AzureServiceBus.Tests;

public class azureservicebusqueuemessagecountthresholdhealthcheck_should
{
    private const string HEALTH_CHECK_NAME = "unit-test-check";

    private readonly string ConnectionString;
    private readonly string FullyQualifiedName;
    private readonly string QueueName;
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
    private readonly TokenCredential _tokenCredential;

    public azureservicebusqueuemessagecountthresholdhealthcheck_should()
    {
        ConnectionString = Guid.NewGuid().ToString();
        FullyQualifiedName = Guid.NewGuid().ToString();
        QueueName = Guid.NewGuid().ToString();

        _clientProvider = Substitute.For<ServiceBusClientProvider>();
        _serviceBusAdministrationClient = Substitute.For<ServiceBusAdministrationClient>();
        _tokenCredential = Substitute.For<TokenCredential>();

        _clientProvider.CreateManagementClient(ConnectionString).Returns(_serviceBusAdministrationClient);
        _clientProvider.CreateManagementClient(FullyQualifiedName, _tokenCredential).Returns(_serviceBusAdministrationClient);
    }

    [Fact]
    public async Task can_create_client_with_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(QueueName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(ConnectionString);
    }

    [Fact]
    public async Task reuses_existing_client_when_using_same_connection_string_with_different_queue()
    {
        // First call
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(QueueName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        // Second call
        var otherQueueName = Guid.NewGuid().ToString();
        var otherOptions = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(otherQueueName)
        {
            ConnectionString = ConnectionString,
        };
        var otherHealthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var otherContext = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };

        var otherActual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(ConnectionString);
    }

    [Fact]
    public async Task can_create_client_with_fully_qualified_endpoint()
    {
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(QueueName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(options.FullyQualifiedNamespace, options.Credential);
    }

    [Fact]
    public async Task reuses_existing_client_when_using_same_fully_qualified_name_with_different_queue()
    {
        // First call
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(QueueName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        // Second call
        var otherQueueName = Guid.NewGuid().ToString();
        var otherOptions = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(otherQueueName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var otherHealthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(otherOptions, _clientProvider);
        var otherContext = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, otherHealthCheck, HealthStatus.Unhealthy, null)
        };

        var otherActual = await otherHealthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(options.FullyQualifiedNamespace, options.Credential);
    }

    [Fact]
    public async Task return_healthy_when_active_queue_threshold_is_null()
    {
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(QueueName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };
        var queueProperties = ServiceBusModelFactory.QueueRuntimeProperties(QueueName);
        var response = Response.FromValue(queueProperties, Substitute.For<Response>());

        _serviceBusAdministrationClient
            .GetQueueRuntimePropertiesAsync(QueueName, tokenSource.Token)
            .Returns(response);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Theory]
    [InlineData(00, 5, 10, HealthStatus.Healthy)]
    [InlineData(04, 5, 10, HealthStatus.Healthy)]
    [InlineData(05, 5, 10, HealthStatus.Degraded)]
    [InlineData(09, 5, 10, HealthStatus.Degraded)]
    [InlineData(10, 5, 10, HealthStatus.Unhealthy)]
    [InlineData(15, 5, 10, HealthStatus.Unhealthy)]
    public async Task return_expected_health_status_based_on_active_message_threshold_count(
        int messageCount,
        int degradedThreshold,
        int unhealthyThreshold,
        HealthStatus expectedHealthStatus)
    {
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(QueueName)
        {
            ConnectionString = ConnectionString,
            ActiveMessages = new AzureServiceBusQueueMessagesCountThreshold
            {
                DegradedThreshold = degradedThreshold,
                UnhealthyThreshold = unhealthyThreshold,
            }
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };
        var queueProperties = ServiceBusModelFactory.QueueRuntimeProperties(QueueName, activeMessageCount: messageCount);
        var response = Response.FromValue(queueProperties, Substitute.For<Response>());

        _serviceBusAdministrationClient
            .GetQueueRuntimePropertiesAsync(QueueName, tokenSource.Token)
            .Returns(response);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(expectedHealthStatus);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Theory]
    [InlineData(00, 5, 10, HealthStatus.Healthy)]
    [InlineData(04, 5, 10, HealthStatus.Healthy)]
    [InlineData(05, 5, 10, HealthStatus.Degraded)]
    [InlineData(09, 5, 10, HealthStatus.Degraded)]
    [InlineData(10, 5, 10, HealthStatus.Unhealthy)]
    [InlineData(15, 5, 10, HealthStatus.Unhealthy)]
    public async Task return_expected_health_status_based_on_dead_letter_message_threshold_count(
        int messageCount,
        int degradedThreshold,
        int unhealthyThreshold,
        HealthStatus expectedHealthStatus)
    {
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(QueueName)
        {
            ConnectionString = ConnectionString,
            DeadLetterMessages = new AzureServiceBusQueueMessagesCountThreshold
            {
                DegradedThreshold = degradedThreshold,
                UnhealthyThreshold = unhealthyThreshold,
            }
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };
        var queueProperties = ServiceBusModelFactory.QueueRuntimeProperties(QueueName, deadLetterMessageCount: messageCount);
        var response = Response.FromValue(queueProperties, Substitute.For<Response>());

        _serviceBusAdministrationClient
            .GetQueueRuntimePropertiesAsync(QueueName, tokenSource.Token)
            .Returns(response);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(expectedHealthStatus);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(QueueName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }
}
