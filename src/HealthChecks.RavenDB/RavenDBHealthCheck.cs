using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.Exceptions.Routing;
using Raven.Client.Http;
using Raven.Client.ServerWide.Operations;
using Sparrow.Json;

namespace HealthChecks.RavenDB
{
    public class RavenDBHealthCheck : IHealthCheck
    {
        private readonly RavenDBOptions _options;

        private static readonly GetBuildNumberOperation _serverHealthCheck = new();

        private static readonly DatabaseHealthCheckOperation _databaseHealthCheck = new();

        private static readonly GetStatisticsOperation _legacyDatabaseHealthCheck = new();

        private static readonly ConcurrentDictionary<RavenDBOptions, DocumentStoreHolder> _stores = new();

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
                var value = _stores.GetOrAdd(_options, o =>
                {
                    var store = new DocumentStore
                    {
                        Urls = o.Urls,
                        Certificate = o.Certificate
                    };

                    try
                    {
                        store.Initialize();
                        return new DocumentStoreHolder
                        {
                            Store = store,
                            Legacy = false
                        };
                    }
                    catch (Exception)
                    {
                        try
                        {
                            store.Dispose();
                        }
                        catch
                        {
                            // nothing that we can do
                        }

                        throw;
                    }
                });

                var store = value.Store;

                if (string.IsNullOrWhiteSpace(_options.Database))
                {
                    await CheckServerHealthAsync(store, cancellationToken);

                    return HealthCheckResult.Healthy();
                }

                try
                {
                    try
                    {
                        await CheckDatabaseHealthAsync(store, _options.Database!, value.Legacy, cancellationToken);
                    }
                    catch (ClientVersionMismatchException e) when (e.Message.Contains(nameof(RouteNotFoundException)))
                    {
                        value.Legacy = true;
                        await CheckDatabaseHealthAsync(store, _options.Database!, value.Legacy, cancellationToken);
                    }

                    return HealthCheckResult.Healthy();
                }
                catch (DatabaseDoesNotExistException)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, $"RavenDB does not contain '{_options.Database}' database.");
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private static Task CheckServerHealthAsync(IDocumentStore store, CancellationToken cancellationToken)
        {
            return store.Maintenance
                .Server
                .SendAsync(_serverHealthCheck, cancellationToken);
        }

        private static async Task CheckDatabaseHealthAsync(IDocumentStore store, string database, bool legacy, CancellationToken cancellationToken)
        {
            var executor = store.Maintenance.ForDatabase(database);

            if (legacy)
            {
                await executor.SendAsync(_legacyDatabaseHealthCheck, cancellationToken);
                return;
            }

            await executor.SendAsync(_databaseHealthCheck, cancellationToken);
        }

        private class DatabaseHealthCheckOperation : IMaintenanceOperation
        {
            public RavenCommand GetCommand(DocumentConventions conventions, JsonOperationContext context)
            {
                return new DatabaseHealthCheckCommand();
            }

            private class DatabaseHealthCheckCommand : RavenCommand
            {
                public DatabaseHealthCheckCommand()
                {
                    Timeout = TimeSpan.FromSeconds(15); // maybe even less?
                }

                public override HttpRequestMessage CreateRequest(JsonOperationContext ctx, ServerNode node, out string url)
                {
                    url = $"{node.Url}/databases/{node.Database}/healthcheck";

                    return new HttpRequestMessage
                    {
                        Method = HttpMethod.Get
                    };
                }
            }
        }

        private class DocumentStoreHolder
        {
            public IDocumentStore Store { get; set; } = null!;

            public bool Legacy { get; set; }
        }
    }
}
