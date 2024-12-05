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
    /// Add a health check for MongoDb that list all databases from specified <paramref name="mongoClientFactory"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="mongoClientFactory">
    /// An optional factory to obtain <see cref="IMongoClient" /> instance.
    /// When not provided, <see cref="MongoClient" /> or <see cref="IMongoClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="mongoDatabaseNameFactory">An optional factory to obtain the name of the database to check.</param>
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
        Func<IServiceProvider, IMongoClient>? mongoClientFactory = default,
        Func<IServiceProvider, string>? mongoDatabaseNameFactory = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new MongoDbHealthCheck(
                // The user might have registered a factory for MongoClient type, but not for the abstraction (IMongoClient).
                // That is why we try to resolve MongoClient first.
                client: mongoClientFactory?.Invoke(sp) ?? sp.GetService<MongoClient>() ?? sp.GetRequiredService<IMongoClient>(),
                databaseName: mongoDatabaseNameFactory?.Invoke(sp)),
            failureStatus,
            tags,
            timeout));
    }
}
