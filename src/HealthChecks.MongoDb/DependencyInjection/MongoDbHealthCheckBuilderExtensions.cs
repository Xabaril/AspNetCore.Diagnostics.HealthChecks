using HealthChecks.MongoDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="MongoDbHealthCheck"/>.
/// </summary>
public static class MongoDbHealthCheckBuilderExtensions
{
    private const string NAME = "mongodb";

    /// <summary>
    /// Add a health check for MongoDb that list all databases from specified <paramref name="clientFactory"/> or pings the database returned by <paramref name="databaseNameFactory"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="IMongoClient" /> instance.
    /// When not provided, <see cref="MongoClient" /> or <see cref="IMongoClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="databaseNameFactory">An optional factory to obtain the name of the database to ping.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mongodb' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMongoDb(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IMongoClient>? clientFactory = default,
        Func<IServiceProvider, string>? databaseNameFactory = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => Factory(sp, clientFactory, databaseNameFactory),
            failureStatus,
            tags,
            timeout));

        static MongoDbHealthCheck Factory(IServiceProvider sp, Func<IServiceProvider, IMongoClient>? clientFactory, Func<IServiceProvider, string>? databaseNameFactory)
        {
            // The user might have registered a factory for MongoClient type, but not for the abstraction (IMongoClient).
            // That is why we try to resolve MongoClient first.
            IMongoClient client = clientFactory?.Invoke(sp) ?? sp.GetService<MongoClient>() ?? sp.GetRequiredService<IMongoClient>();
            string? databaseName = databaseNameFactory?.Invoke(sp);
            return new(client, databaseName);
        }
    }

    /// <summary>
    /// Add a health check for MongoDb that pings the database.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="dbFactory">A factory to obtain <see cref="IMongoDatabase" /> instance.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'mongodb' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMongoDb(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IMongoDatabase> dbFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(dbFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                IMongoDatabase db = dbFactory.Invoke(sp);
                return new MongoDbHealthCheck(db.Client, db.DatabaseNamespace.DatabaseName);
            },
            failureStatus,
            tags,
            timeout));
    }
}
