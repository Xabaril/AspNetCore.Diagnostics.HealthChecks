using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.EventStore
{
    public class EventStoreHealthCheck : IHealthCheck
    {
        private const string CONNECTION_NAME = "AspNetCore HealthCheck Connection";
        private const int ELAPSED_DELAY_MILLISECONDS = 500;
        private const int RECONNECTION_LIMIT = 1;

        private readonly IEventStoreConnection? _eventStoreConnection;
        private readonly string? _eventStoreConnectionString;
        private readonly string? _login;
        private readonly string? _password;

        public EventStoreHealthCheck(string eventStoreConnection, string? login, string? password)
        {
            _eventStoreConnectionString = Guard.ThrowIfNull(eventStoreConnection);
            _login = login;
            _password = password;
        }

        public EventStoreHealthCheck(IEventStoreConnection eventStoreConnection)
        {
            _eventStoreConnection = eventStoreConnection ?? throw new ArgumentNullException(nameof(eventStoreConnection));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionSettingsBuilder = ConnectionSettings.Create()
                    .LimitReconnectionsTo(RECONNECTION_LIMIT)
                    .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(ELAPSED_DELAY_MILLISECONDS))
                    .SetCompatibilityMode("auto");

                if (!string.IsNullOrEmpty(_login) && !string.IsNullOrEmpty(_password))
                {
                    connectionSettingsBuilder.SetDefaultUserCredentials(new UserCredentials(_login, _password));
                }

                using (var connection = _eventStoreConnection ??
                                        EventStoreConnection.Create(
                                            _eventStoreConnectionString,
                                            connectionSettingsBuilder,
                                            CONNECTION_NAME))
                {

                    var tcs = new TaskCompletionSource<HealthCheckResult>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                    //connected
                    connection.Connected += (s, e) => tcs.TrySetResult(HealthCheckResult.Healthy());

                    //connection closed after configured amount of failed reconnections
                    connection.Closed += (s, e) =>
                    {
                        tcs.TrySetResult(new HealthCheckResult(
                            status: context.Registration.FailureStatus,
                            description: e.Reason));
                    };

                    //connection error
                    connection.ErrorOccurred += (s, e) =>
                    {
                        tcs.TrySetResult(new HealthCheckResult(
                            status: context.Registration.FailureStatus,
                            exception: e.Exception));
                    };

                    using (cancellationToken.Register(() => connection.Close()))
                    {
                        //completes after tcp connection init, but before successful connection and login
                        await connection.ConnectAsync();
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                    {
                        return await tcs.Task;
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
