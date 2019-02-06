using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.MongoDb
{
    public class MongoDbHealthCheck
        : IHealthCheck
    {
        private readonly string _specifiedDatabase;
        private readonly IMongoClient _mongoClient;

        public MongoDbHealthCheck(string connectionString, string databaseName = default) : this(MongoClientSettings.FromUrl(MongoUrl.Create(connectionString)), databaseName)
        {
        }
        public MongoDbHealthCheck(MongoClientSettings clientSettings, string databaseName = default)
        {
            _specifiedDatabase = databaseName;
            _mongoClient = new MongoClient(clientSettings);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(_specifiedDatabase))
                {
                    // some users can't list all databases depending on database privileges, with
                    // this you can list only collection on specified database.
                    // Related with issue #43

                    await _mongoClient
                        .GetDatabase(_specifiedDatabase)
                        .ListCollectionsAsync();
                }
                else
                {
                    await _mongoClient
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