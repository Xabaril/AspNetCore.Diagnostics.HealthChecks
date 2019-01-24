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
        private readonly string _connectionString;
        private readonly string _specifiedDatabase;
        private readonly TimeSpan _timeout;

        public MongoDbHealthCheck(string connectionString, TimeSpan timeout, string databaseName = default)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _timeout = timeout;
            _specifiedDatabase = databaseName;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    if (!string.IsNullOrEmpty(_specifiedDatabase))
                    {
                        // some users can't list all databases depending on database privileges, with
                        // this you can list only collection on specified database.
                        // Related with issue #43

                        await new MongoClient(_connectionString)
                            .GetDatabase(_specifiedDatabase)
                            .ListCollectionsAsync(cancellationToken: timeoutCancellationTokenSource.Token);
                    }
                    else
                    {
                        await new MongoClient(_connectionString)
                            .ListDatabasesAsync(timeoutCancellationTokenSource.Token);
                    }

                    return HealthCheckResult.Healthy();
                }
                catch (Exception ex)
                {
                    if (timeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Timeout");
                    }
                    return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                }
            }
        }
    }
}