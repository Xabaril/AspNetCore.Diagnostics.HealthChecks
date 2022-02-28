using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NATS.Client;

namespace HealthChecks.Nats
{
    /// <summary>
    /// Health check for Nats Server
    /// </summary>
    public sealed class NatsHealthCheck : IHealthCheck, IDisposable
    {
        /// <summary>
        ///  Provides factory methods to create connections to NATS Servers.
        /// </summary>
        private static readonly ConnectionFactory _connectionFactory = new ConnectionFactory();

        private readonly NatsOptions _options;

        /// <summary>
        /// An NATS.Client.IConnection object connected to the NATS server.
        /// </summary>
        private IConnection? _connection = null;

        public NatsHealthCheck(NatsOptions natsOptions)
        {
            _options = natsOptions;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _connection = CreateConnection(_options);
                if (_connection is null)
                    throw new ArgumentNullException(nameof(_connection));
                var healthCheckResult = GetHealthCheckResultFromState(_connection);
                return Task.FromResult(healthCheckResult);
            }
            catch (Exception ex)
            {
                var unhealthy = new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                return Task.FromResult(unhealthy);
            }

            IConnection CreateConnection(NatsOptions options)
            {
                if (options == null)
                    throw new ArgumentNullException(nameof(options));
                if (!string.IsNullOrWhiteSpace(options.CredentialsPath))
                    return _connectionFactory.CreateConnection(options.Url, options.CredentialsPath);
                if (!string.IsNullOrWhiteSpace(options.Jwt) && !string.IsNullOrWhiteSpace(options.PrivateNKey))
                    return _connectionFactory.CreateConnection(options.Url, options.Jwt, options.PrivateNKey);
                return _connectionFactory.CreateConnection(options.Url);
            }

            HealthCheckResult GetHealthCheckResultFromState(IConnection connection)
            {
                string description = GetDescription(connection);

                return connection.State switch
                {
                    ConnState.CONNECTED => HealthCheckResult.Healthy(description, GetStatsData(connection)),
                    ConnState.CONNECTING
                    or ConnState.RECONNECTING
                    or ConnState.DRAINING_SUBS
                    or ConnState.DRAINING_PUBS => HealthCheckResult.Degraded(description),
                    ConnState.CLOSED
                    or ConnState.DISCONNECTED => HealthCheckResult.Unhealthy(description),
                    _ => new HealthCheckResult(context.Registration.FailureStatus, description),
                };
            }

            static string GetDescription(IConnection connection)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0}: {1}; ", nameof(connection.ClientIP), connection.ClientIP);
                if (!string.IsNullOrWhiteSpace(connection.ConnectedUrl))
                    sb.AppendFormat("{0}: {1}; ", nameof(connection.ConnectedUrl), connection.ConnectedUrl);
                sb.AppendFormat("{0}: {1}; ", nameof(connection.State), connection.State);
                if (connection.SubscriptionCount != default)
                    sb.AppendFormat("{0}: {1}", nameof(connection.SubscriptionCount), connection.SubscriptionCount);
                var description = sb.ToString();
                return description;
            }

            static IReadOnlyDictionary<string, object> GetStatsData(IConnection connection) =>
                new Dictionary<string, object>
                {
                    [nameof(connection.Stats.InMsgs)] = connection.Stats.InMsgs,
                    [nameof(connection.Stats.OutMsgs)] = connection.Stats.OutMsgs,
                    [nameof(connection.Stats.InBytes)] = connection.Stats.InBytes,
                    [nameof(connection.Stats.OutBytes)] = connection.Stats.OutBytes,
                    [nameof(connection.Stats.Reconnects)] = connection.Stats.Reconnects
                };
        }

        public void Dispose() => _connection?.Dispose();
    }
}
