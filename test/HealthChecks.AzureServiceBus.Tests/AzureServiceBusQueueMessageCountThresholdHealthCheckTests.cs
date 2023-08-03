using Azure;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;
using NSubstitute;

namespace HealthChecks.AzureServiceBus.Tests;

public class azureservicebusqueuemessagecountthresholdhealthcheck_should
{
    private readonly string ConnectionString;
    private readonly string FullyQualifiedName;
    private readonly string TopicName;
    private readonly string HealthCheckName = "unit-test-check";

    private readonly ServiceBusClientProvider _clientProvider;
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
    private readonly TokenCredential _tokenCredential;

    public azureservicebusqueuemessagecountthresholdhealthcheck_should()
    {
        ConnectionString = Guid.NewGuid().ToString();
        FullyQualifiedName = Guid.NewGuid().ToString();
        TopicName = Guid.NewGuid().ToString();

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
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(ConnectionString);
    }

    [Fact]
    public async Task reuses_existing_client_when_using_same_connection_string_with_different_topic()
    {
        // First call
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        // Second call
        var otherTopicName = Guid.NewGuid().ToString();
        var otherOptions = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(otherTopicName)
        {
            ConnectionString = ConnectionString,
        };
        var otherHealthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var otherContext = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
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
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(TopicName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(options.FullyQualifiedNamespace, options.Credential);
    }

    [Fact]
    public async Task reuses_existing_client_when_using_same_fully_qualified_name_with_different_topic()
    {
        // First call
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(TopicName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        // Second call
        var otherTopicName = Guid.NewGuid().ToString();
        var otherOptions = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(otherTopicName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var otherHealthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(otherOptions, _clientProvider);
        var otherContext = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, otherHealthCheck, HealthStatus.Unhealthy, null)
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
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };
        var queueProperties = ServiceBusModelFactory.QueueRuntimeProperties(TopicName);
        var response = Response.FromValue(queueProperties, Substitute.For<Response>());

        _serviceBusAdministrationClient
            .GetQueueRuntimePropertiesAsync(TopicName, tokenSource.Token)
            .Returns(response);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);
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
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
            ActiveMessages = new AzureServiceBusQueueMessagesCountThreshold()
            {
                DegradedThreshold = degradedThreshold,
                UnhealthyThreshold = unhealthyThreshold,
            }
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };
        var queueProperties = ServiceBusModelFactory.QueueRuntimeProperties(TopicName, activeMessageCount: messageCount);
        var response = Response.FromValue(queueProperties, Substitute.For<Response>());

        _serviceBusAdministrationClient
            .GetQueueRuntimePropertiesAsync(TopicName, tokenSource.Token)
            .Returns(response);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(expectedHealthStatus);
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
        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
            ActiveMessages = new AzureServiceBusQueueMessagesCountThreshold(),
            DeadLetterMessages = new AzureServiceBusQueueMessagesCountThreshold()
            {
                DegradedThreshold = degradedThreshold,
                UnhealthyThreshold = unhealthyThreshold,
            }
        };
        var healthCheck = new AzureServiceBusQueueMessageCountThresholdHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };
        var queueProperties = ServiceBusModelFactory.QueueRuntimeProperties(TopicName, activeMessageCount: 0, deadLetterMessageCount: messageCount);
        var response = Response.FromValue(queueProperties, Substitute.For<Response>());

        _serviceBusAdministrationClient
            .GetQueueRuntimePropertiesAsync(TopicName, tokenSource.Token)
            .Returns(response);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetQueueRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(expectedHealthStatus);
    }
}
