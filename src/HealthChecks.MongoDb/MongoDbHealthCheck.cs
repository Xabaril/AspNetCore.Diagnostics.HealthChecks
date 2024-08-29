using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthChecks.MongoDb;

public class MongoDbHealthCheck : IHealthCheck
{
    private static readonly BsonDocumentCommand<BsonDocument> _command = new(BsonDocument.Parse("{ping:1}"));
    private static readonly ConcurrentDictionary<string, IMongoClient> _mongoClient = new();
    private readonly MongoClientSettings _mongoClientSettings;
    private readonly string? _specifiedDatabase;

    public MongoDbHealthCheck(string connectionString, string? databaseName = default)
        : this(MongoClientSettings.FromUrl(MongoUrl.Create(connectionString)), databaseName)
    {
        if (databaseName == default)
        {
            _specifiedDatabase = MongoUrl.Create(connectionString)?.DatabaseName;
        }
    }

    public MongoDbHealthCheck(IMongoClient client, string? databaseName = default)
        : this(client.Settings, databaseName)
    {
        _mongoClient[_mongoClientSettings.ToString()] = client;
    }

    public MongoDbHealthCheck(MongoClientSettings clientSettings, string? databaseName = default)
    {
        _specifiedDatabase = databaseName;
        _mongoClientSettings = clientSettings;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var mongoClient = _mongoClient.GetOrAdd(_mongoClientSettings.ToString(), _ => new MongoClient(_mongoClientSettings));

            if (!string.IsNullOrEmpty(_specifiedDatabase))
            {
                // some users can't list all databases depending on database privileges, with
                // this you can check a specified database.
                // Related with issue #43 and #617

                await mongoClient
                    .GetDatabase(_specifiedDatabase)
                    .RunCommandAsync(_command, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                using var cursor = await mongoClient.ListDatabaseNamesAsync(cancellationToken).ConfigureAwait(false);
                await cursor.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
