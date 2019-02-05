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
        private readonly MongoClientSettings _clientSettings;
        public MongoDbHealthCheck(string connectionString, string databaseName = default)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            _clientSettings = new MongoClient(connectionString).Settings.Clone();
            _specifiedDatabase = databaseName;
        }
        public MongoDbHealthCheck(MongoClientSettings clientSettings, string databaseName = default)
        {
            _clientSettings = clientSettings;
            _specifiedDatabase = databaseName;
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

                    await new MongoClient(_clientSettings)
                        .GetDatabase(_specifiedDatabase)
                        .ListCollectionsAsync();
                }
                else
                {
                    await new MongoClient(_clientSettings)
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