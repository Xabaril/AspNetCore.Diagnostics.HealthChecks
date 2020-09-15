using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.CosmosDb
{
    public class TableServiceHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, TableServiceClient> _connections = new ConcurrentDictionary<string, TableServiceClient>();

        private readonly string _connectionString;
        private readonly string _tableName;

        private readonly Uri _endpoint;
        private readonly TableSharedKeyCredential _credentials;


        public TableServiceHealthCheck(string connectionString, string tableName)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public TableServiceHealthCheck(Uri endpoint, TableSharedKeyCredential credentials, string tableName)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                TableServiceClient tableServiceClient;

                var tableServiceKey = _connectionString ?? _endpoint.ToString();
                if (!_connections.TryGetValue(tableServiceKey, out tableServiceClient))
                {
                    tableServiceClient = CreateTableServiceClient();

                    if (!_connections.TryAdd(tableServiceKey, tableServiceClient))
                    {
                        tableServiceClient = _connections[_connectionString];
                    }
                }

                var tables = tableServiceClient.GetTablesAsync(cancellationToken: cancellationToken);

                await foreach (var item in tables)
                {
                    if (item.TableName.Equals(_tableName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return HealthCheckResult.Healthy();
                    }
                }

                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Table with name {_tableName} does not exist.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        TableServiceClient CreateTableServiceClient()
        {
            if (!String.IsNullOrEmpty(_connectionString))
            {
                return new TableServiceClient(_connectionString);
            }

            return new TableServiceClient(_endpoint, _credentials);
        }
    }
}