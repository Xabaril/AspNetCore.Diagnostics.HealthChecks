using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.EventStore
{
    public class EventStoreHealthCheck : IHealthCheck, IDisposable
    {
        private const string CONNECTION_NAME = "AspNetCore HealthCheck Connection";
        private const int ELAPSED_DELAY_MILLISECONDS = 500;
        private const int RECONNECTION_LIMIT = 1;

        private readonly IEventStoreConnection _eventStoreConnection;

        private readonly TaskCompletionSource<HealthCheckResult> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public EventStoreHealthCheck(string eventStoreConnection, string? login, string? password)
        {
            Guard.ThrowIfNull(eventStoreConnection, throwOnEmptyString: true);
            _eventStoreConnection ??= CreateEventStoreConnection(eventStoreConnection, login, password);
        }

        public EventStoreHealthCheck(IEventStoreConnection eventStoreConnection)
        {
            _eventStoreConnection = Guard.ThrowIfNull(eventStoreConnection);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                //connected
                _eventStoreConnection.Connected += (s, e) => _tcs.TrySetResult(HealthCheckResult.Healthy());

                //connection closed after configured amount of failed reconnections
                _eventStoreConnection.Closed += (s, e) =>
                    {
                        _tcs.TrySetResult(new HealthCheckResult(
                            status: context.Registration.FailureStatus,
                            description: e.Reason));
                    };

                //connection error
                _eventStoreConnection.ErrorOccurred += (s, e) =>
                    {
                        _tcs.TrySetResult(new HealthCheckResult(
                            status: context.Registration.FailureStatus,
                            exception: e.Exception));
                    };

                using (cancellationToken.Register(() => _eventStoreConnection.Close()))
                {
                    try
                    {
                        //completes after tcp connection init, but before successful connection and login
                        await _eventStoreConnection.ConnectAsync();
                    }
                    catch (InvalidOperationException exception)
                    {
                        if (ConnectionIsAlreadyRunning(exception))
                        {
                            _tcs.TrySetResult(HealthCheckResult.Healthy());
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                using (cancellationToken.Register(() => _tcs.TrySetCanceled()))
                {
                    return await _tcs.Task;
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private bool ConnectionIsAlreadyRunning(InvalidOperationException exception)
            => exception.Message == $"EventStoreConnection '{_eventStoreConnection!.ConnectionName}' is already active.";

        private static IEventStoreConnection CreateEventStoreConnection(string eventStoreConnectionString, string? login, string? password)
        {
            var connectionSettingsBuilder = ConnectionSettings.Create()
                .LimitReconnectionsTo(RECONNECTION_LIMIT)
                .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(ELAPSED_DELAY_MILLISECONDS))
                .SetCompatibilityMode("auto");

            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                connectionSettingsBuilder.SetDefaultUserCredentials(new UserCredentials(login, password));
            }

            return EventStoreConnection.Create(
                eventStoreConnectionString,
                connectionSettingsBuilder,
                CONNECTION_NAME);
        }

        public void Dispose() => _eventStoreConnection.Close();
    }
}
