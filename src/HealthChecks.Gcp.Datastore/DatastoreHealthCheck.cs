using Google.Api.Gax.Grpc;
using Google.Cloud.Datastore.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Gcp.Datastore
{
    public class DatastoreHealthCheck
        : IHealthCheck
    {
        private readonly DatastoreDb _datastoreDb;
        private readonly int _timeOut;

        public DatastoreHealthCheck(DatastoreDb datastoreDb, int timeOut)
        {
            _datastoreDb = datastoreDb ?? throw new ArgumentNullException(nameof(datastoreDb));
            _timeOut = timeOut;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var transaction = await _datastoreDb.BeginTransactionAsync(
                        CallSettings.FromCallTiming(
                            CallTiming.FromTimeout(
                                TimeSpan.FromSeconds(_timeOut)))))
                {
                    return HealthCheckResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
