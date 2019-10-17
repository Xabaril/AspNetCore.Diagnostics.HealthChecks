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
        private static readonly ConcurrentDictionary<MongoClientSettings, MongoClient> _mongoClient = new ConcurrentDictionary<MongoClientSettings, MongoClient>();
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
                var mongoClient = _mongoClient.GetOrAdd(_mongoClientSettings, settings => new MongoClient(settings));

                if (!string.IsNullOrEmpty(_specifiedDatabase))
                {
                    // some users can't list all databases depending on database privileges, with
                    // this you can list only collection on specified database.
                    // Related with issue #43

                    await (await mongoClient
                         .GetDatabase(_specifiedDatabase)
                         .ListCollectionsAsync(cancellationToken: cancellationToken)).FirstAsync(cancellationToken);
                }
                else
                {
                    await mongoClient
                        .ListDatabasesAsync(cancellationToken);
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