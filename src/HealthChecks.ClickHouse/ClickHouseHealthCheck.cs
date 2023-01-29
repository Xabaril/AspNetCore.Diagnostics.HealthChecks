using ClickHouse.Client.ADO;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ClickHouse;

public class ClickHouseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly string _sql;
    private readonly Action<ClickHouseConnection>? _setup;

    /// <summary>
    /// Check the ability to connect to the ClickHouse DataBase
    /// </summary>
    /// <param name="clickHouseConnectionString">ClickHouse DataBase connection string</param>
    /// <param name="sql">Custom sql-query</param>
    /// <param name="setup">The action to configure the connection</param>
    public ClickHouseHealthCheck(string clickHouseConnectionString, string sql, Action<ClickHouseConnection>? setup = null)
    {
        _connectionString = clickHouseConnectionString ?? throw new ArgumentNullException(nameof(clickHouseConnectionString));
        _sql = sql ?? throw new ArgumentNullException(nameof(sql));
        _setup = setup;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = new ClickHouseConnection(_connectionString))
            {
                _setup?.Invoke(connection);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = _sql;
                    await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
