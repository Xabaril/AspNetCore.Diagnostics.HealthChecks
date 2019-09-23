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
                var eventStoreUri = new Uri(_eventStoreConnection);

                ConnectionSettings connectionSettings;

                if (string.IsNullOrEmpty(_login) || string.IsNullOrEmpty(_password))
                {
                    connectionSettings = ConnectionSettings.Create()
                        .LimitReconnectionsTo(1)
                        .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(500))
                        .Build();
                }
                else
                {
                    connectionSettings = ConnectionSettings.Create()
                        .LimitReconnectionsTo(1)
                        .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(500))
                        .SetDefaultUserCredentials(new UserCredentials(_login, _password))
                        .Build();
                }

                using (var connection = EventStoreConnection.Create(
                    connectionSettings,
                    eventStoreUri,
                    CONNECTION_NAME))
                {
                    var tcs = new TaskCompletionSource<HealthCheckResult>();

                    //connected
                    connection.Connected += (s, e) => tcs.TrySetResult(HealthCheckResult.Healthy());
                    //connection closed after configured amount of failed reconnections
                    connection.Closed += (s, e) => tcs.TrySetResult(new HealthCheckResult(
                        status: context.Registration.FailureStatus,
                        description: e.Reason));
                    //connection error
                    connection.ErrorOccurred += (s, e) => tcs.TrySetResult(new HealthCheckResult(
                        status: context.Registration.FailureStatus,
                        exception: e.Exception));

                    //completes after tcp connection init, but before successful connection and login
                    await connection.ConnectAsync();

                    return await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
