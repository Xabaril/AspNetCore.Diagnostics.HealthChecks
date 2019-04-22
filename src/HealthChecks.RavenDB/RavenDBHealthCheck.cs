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
        private readonly RavenDBOptions _options;
        public RavenDBHealthCheck(RavenDBOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Urls == null)
            {
                throw new ArgumentNullException(nameof(options.Urls));
            }

            _options = options;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var store = new DocumentStore
                {
                    Urls = _options.Urls,
                })
                {
                    if (_options.Certificate != null)
                    {
                        store.Certificate = _options.Certificate;
                    }

                    store.Initialize();

                    var databases = await store
                        .Maintenance
                        .Server
                        .SendAsync(new GetDatabaseNamesOperation(start: 0, pageSize: 100), cancellationToken);

                    if (!string.IsNullOrWhiteSpace(_options.Database)
                        && !databases.Contains(_options.Database, StringComparer.OrdinalIgnoreCase))
                    {
                        return new HealthCheckResult(
                            context.Registration.FailureStatus,
                            $"RavenDB does not contain '{_options.Database}' database.");
                    }

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
