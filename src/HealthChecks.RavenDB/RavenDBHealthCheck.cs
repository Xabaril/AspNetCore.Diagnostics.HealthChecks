using Microsoft.Extensions.Diagnostics.HealthChecks;
using Raven.Client.Documents;
using Raven.Client.ServerWide.Operations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.RavenDB
{
    public class RavenDBHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string[] _urls;
        private readonly string _specifiedDatabase;

        public RavenDBHealthCheck(string connectionString, string databaseName = default)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _specifiedDatabase = databaseName;
        }

        public RavenDBHealthCheck(string[] urls, string databaseName = default)
        {
            if (urls.Length == 0)
            {
                throw new ArgumentException("At least 1 Raven url connection string is required.", nameof(urls));
            }

            _urls = urls;
            _specifiedDatabase = databaseName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var store = new DocumentStore
                {
                    Urls = _urls ?? new string[] { _connectionString }
                })
                {
                    store.Initialize();
                    var databases = await store.Maintenance.Server.SendAsync(new GetDatabaseNamesOperation(start: 0, pageSize: 100));

                    if (!string.IsNullOrWhiteSpace(_specifiedDatabase)
                        && !databases.Contains(_specifiedDatabase, StringComparer.OrdinalIgnoreCase))
                    {
                        return new HealthCheckResult(
                            context.Registration.FailureStatus,
                            $"RavenDB doesn't contains '{_specifiedDatabase}' database.");
                    }
                    else
                    {
                        return HealthCheckResult.Healthy();
                    }
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
