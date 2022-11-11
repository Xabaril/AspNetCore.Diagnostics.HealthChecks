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
            _eventStoreConnection.Connected += OnConnectionConnected();
        }

        public EventStoreHealthCheck(IEventStoreConnection eventStoreConnection)
        {
            _eventStoreConnection = Guard.ThrowIfNull(eventStoreConnection);
            _eventStoreConnection.Connected += OnConnectionConnected();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _eventStoreConnection.Closed += OnConnectionClosed(context);
                _eventStoreConnection.ErrorOccurred += OnConnectionErrorOccurred(context);

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
                    var healthCheckResult = await _tcs.Task;
                    _eventStoreConnection.Closed -= OnConnectionClosed(context);
                    _eventStoreConnection.ErrorOccurred -= OnConnectionErrorOccurred(context);
                    return healthCheckResult;
                }
            }
            catch (Exception ex)
            {
                _eventStoreConnection.Closed -= OnConnectionClosed(context);
                _eventStoreConnection.ErrorOccurred -= OnConnectionErrorOccurred(context);
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        /// <summary>
        /// <see cref="IEventStoreConnection.ErrorOccurred"/> is invoked when an error is thrown on the <see cref="_eventStoreConnection"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private EventHandler<ClientErrorEventArgs> OnConnectionErrorOccurred(HealthCheckContext context) =>
            (s, e) =>
            {
                _tcs.TrySetResult(new HealthCheckResult(
                    status: context.Registration.FailureStatus,
                    exception: e.Exception));
            };

        /// <summary>
        /// <see cref="IEventStoreConnection.Connected"/> is invoked when the <see cref="_eventStoreConnection"/> has established a connection to EventStore.
        /// </summary>
        /// <returns></returns>
        private EventHandler<ClientConnectionEventArgs> OnConnectionConnected() =>
            (s, e) => _tcs.TrySetResult(HealthCheckResult.Healthy());

        /// <summary>
        /// <see cref="IEventStoreConnection.Closed"/> is invoked after reconnection has failed the configured amount of times.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private EventHandler<ClientClosedEventArgs> OnConnectionClosed(HealthCheckContext context) =>
            (s, e) =>
            {
                _tcs.TrySetResult(new HealthCheckResult(
                    status: context.Registration.FailureStatus,
                    description: e.Reason));
            };

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
