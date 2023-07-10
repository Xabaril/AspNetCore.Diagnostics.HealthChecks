using System.Collections.Concurrent;
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

namespace HealthChecks.RavenDB;

/// <summary>
/// A health check for RavenDB.
/// </summary>
public class RavenDBHealthCheck : IHealthCheck
{
    private const int DEFAULT_REQUEST_TIMEOUT_IN_SECONDS = 15;

    private readonly RavenDBOptions _options;

    private static readonly GetBuildNumberOperation _serverHealthCheck = new();

    private static readonly GetStatisticsOperation _legacyDatabaseHealthCheck = new();

    private static readonly ConcurrentDictionary<RavenDBOptions, DocumentStoreHolder> _stores = new();

    public RavenDBHealthCheck(RavenDBOptions options)
    {
        Guard.ThrowIfNull(options);
        Guard.ThrowIfNull(options.Urls);

        _options = options;
    }

    /// <inheritdoc />
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
                    _requestTimeout = _options.RequestTimeout ?? TimeSpan.FromSeconds(DEFAULT_REQUEST_TIMEOUT_IN_SECONDS);
                    SetRequestTimeoutForStoreForSpecificDatabase(store);

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
                await CheckServerHealthAsync(store, cancellationToken).ConfigureAwait(false);

                return HealthCheckResult.Healthy();
            }

            try
            {
                try
                {
                    await CheckDatabaseHealthAsync(store, _options.Database!, value.Legacy, cancellationToken).ConfigureAwait(false);
                }
                catch (ClientVersionMismatchException e) when (e.Message.Contains(nameof(RouteNotFoundException)))
                {
                    value.Legacy = true;
                    await CheckDatabaseHealthAsync(store, _options.Database!, value.Legacy, cancellationToken).ConfigureAwait(false);
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

    private void SetRequestTimeoutForStoreForSpecificDatabase(IDocumentStore store)
    {
        if (!string.IsNullOrWhiteSpace(_options.Database))
        {
            store?.SetRequestTimeout(_requestTimeout, _options.Database);
        }
    }

    private static Task CheckServerHealthAsync(IDocumentStore store, CancellationToken cancellationToken)
    {
        return store.Maintenance
            .Server
            .SendAsync(_serverHealthCheck, cancellationToken);
    }

    private async Task CheckDatabaseHealthAsync(IDocumentStore store, string database, bool legacy, CancellationToken cancellationToken)
    {
        var executor = store.Maintenance.ForDatabase(database);

        if (legacy)
        {
            await executor.SendAsync(_legacyDatabaseHealthCheck, cancellationToken).ConfigureAwait(false);
            return;
        }

        await executor.SendAsync(new DatabaseHealthCheckOperation(_requestTimeout), cancellationToken).ConfigureAwait(false);
    }

    private class DatabaseHealthCheckOperation : IMaintenanceOperation
    {
        private readonly TimeSpan _timeout;

        public DatabaseHealthCheckOperation(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        public RavenCommand GetCommand(DocumentConventions conventions, JsonOperationContext context)
        {
            return new DatabaseHealthCheckCommand(_timeout);
        }

        private class DatabaseHealthCheckCommand : RavenCommand
        {
            public DatabaseHealthCheckCommand(TimeSpan timeout)
            {
                Timeout = timeout;
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
