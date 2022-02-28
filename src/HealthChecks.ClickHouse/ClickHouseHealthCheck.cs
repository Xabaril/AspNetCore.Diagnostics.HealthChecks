using ClickHouse.Client.ADO;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ClickHouse;

public class ClickHouseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly string _sql;
    private readonly Action<ClickHouseConnection>? _connection;

    /// <summary>
    /// Check the ability to connect to the ClickHouse DataBase
    /// </summary>
    /// <param name="clickHouseConnectionString">ClickHouse DataBase connection string</param>
    /// <param name="sql">Custom sql-query</param>
    /// <param name="connection">ClickHouse.Client.ADO.ClickHouseConnection</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ClickHouseHealthCheck(string clickHouseConnectionString, string sql, Action<ClickHouseConnection>? connection = null)
    {
        _connectionString = clickHouseConnectionString ?? throw new ArgumentNullException(nameof(clickHouseConnectionString));
        _sql = sql ?? throw new ArgumentNullException(nameof(sql));
        _connection = connection;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var connection = new ClickHouseConnection(_connectionString))
            {
                _connection?.Invoke(connection);

                await connection.OpenAsync(cancellationToken);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = _sql;
                    await command.ExecuteScalarAsync(cancellationToken);
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
