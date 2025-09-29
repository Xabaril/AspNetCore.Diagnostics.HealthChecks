namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureEventGridHealthCheck"/>.
/// </summary>
public static class AzureEventGridHealthChecksBuilderExtensions
{
    private const string NAME = "azure_event_grid";

    /// <summary>
    /// Add a health check for Azure Event Grid by registering <see cref="AzureEventGridHealthCheck"/> for given <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add <see cref="HealthCheckRegistration"/> to.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="EventGridPublisherClient" /> instance.
    /// When not provided, <see cref="EventGridPublisherClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the name 'azure_event_grid' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureEventGrid(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, EventGridPublisherClient>? clientFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           name ?? NAME,
           sp => new AzureEventGridHealthCheck(clientFactory?.Invoke(sp) ?? sp.GetRequiredService<EventGridPublisherClient>()),
           failureStatus,
           tags,
           timeout));
    }
}