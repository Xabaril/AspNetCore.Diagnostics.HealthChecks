using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.EventStore
{
    public class EventStoreHealthCheck
        : IHealthCheck
    {
        const string CONNECTION_NAME = "AspNetCore HealthCheck Connection";
        const int ELAPSED_DELAY_MILLISECONDS = 500;
        const int RECONNECTION_LIMIT = 1;

        private readonly string _eventStoreConnection;
        private readonly string _login;
        private readonly string _password;
        public EventStoreHealthCheck(string eventStoreConnection, string login, string password)
        {
            _eventStoreConnection = eventStoreConnection ?? throw new ArgumentNullException(nameof(eventStoreConnection));
            _login = login;
            _password = password;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                ConnectionSettingsBuilder connectionSettings;

                if (string.IsNullOrEmpty(_login) || string.IsNullOrEmpty(_password))
                {
                    connectionSettings = ConnectionSettings.Create()
                        .LimitReconnectionsTo(RECONNECTION_LIMIT)
                        .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(ELAPSED_DELAY_MILLISECONDS));
                }
                else
                {
                    connectionSettings = ConnectionSettings.Create()
                        .LimitReconnectionsTo(RECONNECTION_LIMIT)
                        .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(ELAPSED_DELAY_MILLISECONDS))
                        .SetDefaultUserCredentials(new UserCredentials(_login, _password));
                }

                using (var connection = EventStoreConnection.Create(
                    _eventStoreConnection,
                    connectionSettings,
                    CONNECTION_NAME))
                {
                    var tcs = new TaskCompletionSource<HealthCheckResult>();

                    //connected
                    connection.Connected += (s, e) =>
                    {
                        tcs.TrySetResult(HealthCheckResult.Healthy());
                    };

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