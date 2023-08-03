using Azure.Core;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.AzureServiceBus.Tests;

public class azureservicebustopichealthcheck_should
{
    private readonly string ConnectionString;
    private readonly string FullyQualifiedName;
    private readonly string TopicName;
    private readonly string HealthCheckName = "unit-test-check";

    private readonly ServiceBusClientProvider _clientProvider;
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
    private readonly TokenCredential _tokenCredential;

    public azureservicebustopichealthcheck_should()
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
        var options = new AzureServiceBusTopicHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
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
    public async Task reuses_existing_client_when_using_same_connection_string_with_different_queue()
    {
        // First call
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusTopicHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        // Second call
        var otherTopicName = Guid.NewGuid().ToString();
        var otherOptions = new AzureServiceBusTopicHealthCheckOptions(otherTopicName)
        {
            ConnectionString = ConnectionString,
        };
        var otherHealthCheck = new AzureServiceBusTopicHealthCheck(otherOptions, _clientProvider);
        var otherContext = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, otherHealthCheck, HealthStatus.Unhealthy, null)
        };

        var otherActual = await otherHealthCheck
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
        var options = new AzureServiceBusTopicHealthCheckOptions(TopicName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
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
    public async Task reuses_existing_client_when_checking_different_topic_in_same_servicebus()
    {
        // First call
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusTopicHealthCheckOptions(TopicName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        // Second call
        var otherTopicName = Guid.NewGuid().ToString();
        var otherOptions = new AzureServiceBusTopicHealthCheckOptions(otherTopicName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var otherHealthCheck = new AzureServiceBusTopicHealthCheck(otherOptions, _clientProvider);
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
    public async Task return_healthy_when_only_checking_healthy_service_through_administration_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusTopicHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
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

        await _serviceBusAdministrationClient
            .Received(1)
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service_through_administration_and_endpoint()
    {
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusTopicHealthCheckOptions(TopicName)
        {
            FullyQualifiedNamespace = FullyQualifiedName,
            Credential = _tokenCredential,
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(FullyQualifiedName, options.Credential);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task return_unhealthy_when_exception_is_thrown_by_administration_client()
    {
        using var tokenSource = new CancellationTokenSource();
        var options = new AzureServiceBusTopicHealthCheckOptions(TopicName)
        {
            ConnectionString = ConnectionString,
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HealthCheckName, healthCheck, HealthStatus.Unhealthy, null)
        };

        _serviceBusAdministrationClient
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        await _serviceBusAdministrationClient
           .Received(1)
           .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
           .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);
    }
}
