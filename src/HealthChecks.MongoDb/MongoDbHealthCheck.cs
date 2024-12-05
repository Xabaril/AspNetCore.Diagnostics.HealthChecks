using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthChecks.MongoDb;

public class MongoDbHealthCheck : IHealthCheck
{
    private const int MAX_PING_ATTEMPTS = 2;

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

                // For most operations where it is possible, the MongoDB driver itself will retry exactly once
                // to cover switches in the primary and temporary short term network outages.
                // Due to the RunCommand being a lower level function, according to the spec (https://github.com/mongodb/specifications/blob/master/source/run-command/run-command.rst#retryability)
                // for it, it is not retryable and this extends to the ping.
                for (int attempt = 1; attempt <= MAX_PING_ATTEMPTS; attempt++)

                {
                    try
                    {
                        await mongoClient
                            .GetDatabase(_specifiedDatabase)
                            .RunCommandAsync(_command, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        if (MAX_PING_ATTEMPTS == attempt)
                        {
                            throw;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
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
