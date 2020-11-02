using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.MongoDb
{
    public class MongoDbHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, MongoClient> _mongoClient = new ConcurrentDictionary<string, MongoClient>();
        private readonly MongoClientSettings _mongoClientSettings;
        private readonly string _specifiedDatabase;
        public MongoDbHealthCheck(string connectionString, string databaseName = default)
            : this(MongoClientSettings.FromUrl(MongoUrl.Create(connectionString)), databaseName)
        {
            if (databaseName == default)
            {
                _specifiedDatabase = MongoUrl.Create(connectionString)?.DatabaseName;
            }
        }
        public MongoDbHealthCheck(MongoClientSettings clientSettings, string databaseName = default)
        {
            _specifiedDatabase = databaseName;
            _mongoClientSettings = clientSettings;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var mongoClient = _mongoClient.GetOrAdd(_mongoClientSettings.ToString(), _ => new MongoClient(_mongoClientSettings));

                if (!string.IsNullOrEmpty(_specifiedDatabase))
                {
                    // some users can't list all databases depending on database privileges, with
                    // this you can list only collections on specified database.
                    // Related with issue #43

                    using var cursor = await mongoClient
                        .GetDatabase(_specifiedDatabase)
                        .ListCollectionNamesAsync(cancellationToken: cancellationToken);
                    await cursor.FirstAsync(cancellationToken);
                }
                else
                {
                    using var cursor = await mongoClient.ListDatabaseNamesAsync(cancellationToken);
                    await cursor.FirstOrDefaultAsync(cancellationToken);
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
