using Azure.Core;
using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure
/// <see cref="AzureServiceBusHealthCheck{TOptions}"/>, <see cref="AzureServiceBusQueueHealthCheck"/>,
/// <see cref="AzureServiceBusSubscriptionHealthCheck"/>, <see cref="AzureServiceBusTopicHealthCheck"/>,
/// <see cref="AzureServiceBusQueueMessageCountThresholdHealthCheck"/>.
/// </summary>
public static class AzureServiceBusHealthCheckBuilderExtensions
{
    private const string AZUREQUEUE_NAME = "azurequeue";
    private const string AZURETOPIC_NAME = "azuretopic";
    private const string AZURESUBSCRIPTION_NAME = "azuresubscription";
    private const string AZUREQUEUETHRESHOLD_NAME = "azurequeuethreshold";

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The azure service bus connection string to be used.</param>
    /// <param name="queueName">The name of the queue to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueue(
        this IHealthChecksBuilder builder,
        string connectionString,
        string queueName,
        Action<AzureServiceBusQueueHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusQueue(
            _ => connectionString,
            _ => queueName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the azure service bus connection string to be used.</param>
    /// <param name="queueNameFactory">A factory to build the queue name to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueue(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> queueNameFactory,
        Action<AzureServiceBusQueueHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(queueNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREQUEUE_NAME,
            sp =>
            {
                var options = new AzureServiceBusQueueHealthCheckOptions(queueNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusQueueHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespace">The azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="queueName">The name of the queue to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueue(
        this IHealthChecksBuilder builder,
        string fullyQualifiedNamespace,
        string queueName,
        TokenCredential tokenCredential,
        Action<AzureServiceBusQueueHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusQueue(
            _ => fullyQualifiedNamespace,
            _ => queueName,
            _ => tokenCredential,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespaceFactory">A factory to build the azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="queueNameFactory">A factory to build the name of the queue to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build the token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueue(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> fullyQualifiedNamespaceFactory,
        Func<IServiceProvider, string> queueNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusQueueHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(fullyQualifiedNamespaceFactory);
        Guard.ThrowIfNull(queueNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREQUEUE_NAME,
            sp =>
            {
                var options = new AzureServiceBusQueueHealthCheckOptions(queueNameFactory(sp))
                {
                    FullyQualifiedNamespace = fullyQualifiedNamespaceFactory(sp),
                    Credential = tokenCredentialFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusQueueHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue active or dead letter messages threshold.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The azure service bus connection string to be used.</param>
    /// <param name="queueName">The name of the queue to check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeuethreshold' will be used for the name.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueueMessageCountThreshold(
        this IHealthChecksBuilder builder,
        string connectionString,
        string queueName,
        string? name = default,
        Action<AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions>? configure = null,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionString);
        Guard.ThrowIfNull(queueName);

        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(queueName)
        {
            ConnectionString = connectionString,
        };

        configure?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREQUEUETHRESHOLD_NAME,
            sp => new AzureServiceBusQueueMessageCountThresholdHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue active or dead letter messages threshold.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpoint">The azure service bus endpoint to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="queueName">The name of the queue to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeuethreshold' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueueMessageCountThreshold(
        this IHealthChecksBuilder builder,
        string endpoint,
        string queueName,
        TokenCredential tokenCredential,
        Action<AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions>? configure = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(endpoint);
        Guard.ThrowIfNull(queueName);
        Guard.ThrowIfNull(tokenCredential);

        var options = new AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(queueName)
        {
            FullyQualifiedNamespace = endpoint,
            Credential = tokenCredential,
        };

        configure?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREQUEUETHRESHOLD_NAME,
            sp => new AzureServiceBusQueueMessageCountThresholdHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Topic.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure ServiceBus connection string to be used.</param>
    /// <param name="topicName">The name of the topic to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopic(
        this IHealthChecksBuilder builder,
        string connectionString,
        string topicName,
        Action<AzureServiceBusTopicHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusTopic(
            _ => connectionString,
            _ => topicName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for Azure Service Bus Topic.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Azure ServiceBus connection string to be used.</param>
    /// <param name="topicNameFactory">A factory to build the name of the topic to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopic(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Action<AzureServiceBusTopicHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(topicNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURETOPIC_NAME,
            sp =>
            {
                var options = new AzureServiceBusTopicHealthCheckOptions(topicNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusTopicHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Topic.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespace">The azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicName">The name of the topic to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopic(
        this IHealthChecksBuilder builder,
        string fullyQualifiedNamespace,
        string topicName,
        TokenCredential tokenCredential,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusTopic(
            _ => fullyQualifiedNamespace,
            _ => topicName,
            _ => tokenCredential,
            configure: null,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for Azure Service Bus Topic.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespaceFactory">A factory to build the azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicNameFactory">A factory to build the name of the topic to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build the token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopic(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> fullyQualifiedNamespaceFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusTopicHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(fullyQualifiedNamespaceFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURETOPIC_NAME,
            sp =>
            {
                var options = new AzureServiceBusTopicHealthCheckOptions(topicNameFactory(sp))
                {
                    FullyQualifiedNamespace = fullyQualifiedNamespaceFactory(sp),
                    Credential = tokenCredentialFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusTopicHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure ServiceBus connection string to be used.</param>
    /// <param name="topicName">The name of the topic to check.</param>
    /// <param name="subscriptionName">The subscription name of the topic subscription to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        string connectionString,
        string topicName,
        string subscriptionName,
        Action<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusSubscription(
            _ => connectionString,
            _ => topicName,
            _ => subscriptionName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Azure ServiceBus connection string to be used.</param>
    /// <param name="topicNameFactory">A factory to build the name of the topic to check.</param>
    /// <param name="subscriptionNameFactory">A factory to build the subscription name of the topic subscription to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, string> subscriptionNameFactory,
        Action<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(subscriptionNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURESUBSCRIPTION_NAME,
            sp =>
            {
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(topicNameFactory(sp), subscriptionNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusSubscriptionHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespace">The azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicName">The name of the topic to check.</param>
    /// <param name="subscriptionName">The subscription name of the topic subscription to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        string fullyQualifiedNamespace,
        string topicName,
        string subscriptionName,
        TokenCredential tokenCredential,
        Action<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusSubscription(
            _ => fullyQualifiedNamespace,
            _ => topicName,
            _ => subscriptionName,
            _ => tokenCredential,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespaceFactory">A factory to build the azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicNameFactory">A factory to build the name of the topic to check.</param>
    /// <param name="subscriptionNameFactory">A factory to build the subscription name of the topic subscription to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build the token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> fullyQualifiedNamespaceFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, string> subscriptionNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(fullyQualifiedNamespaceFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(subscriptionNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURESUBSCRIPTION_NAME,
            sp =>
            {
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(topicNameFactory(sp), subscriptionNameFactory(sp))
                {
                    FullyQualifiedNamespace = fullyQualifiedNamespaceFactory(sp),
                    Credential = tokenCredentialFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusSubscriptionHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }
}
