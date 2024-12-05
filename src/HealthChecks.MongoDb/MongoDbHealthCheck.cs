using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthChecks.MongoDb;

public class MongoDbHealthCheck : IHealthCheck
{
    private const int MAX_PING_ATTEMPTS = 2;

    private static readonly BsonDocumentCommand<BsonDocument> _command = new(BsonDocument.Parse("{ping:1}"));
    private readonly IMongoClient _client;
    private readonly string? _specifiedDatabase;

    public MongoDbHealthCheck(IMongoClient client, string? databaseName = default)
    {
        _client = client;
        _specifiedDatabase = databaseName;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
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
                        await _client
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
                using var cursor = await _client.ListDatabaseNamesAsync(cancellationToken).ConfigureAwait(false);
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
