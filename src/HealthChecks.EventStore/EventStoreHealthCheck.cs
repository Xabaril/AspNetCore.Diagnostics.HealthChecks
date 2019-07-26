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
                        .KeepRetrying()
                        .Build();
                }
                else
                {
                    connectionSettings = ConnectionSettings.Create()
                        .KeepRetrying()
                        .SetDefaultUserCredentials(new UserCredentials(_login, _password))
                        .Build();
                }

                using (var connection = EventStoreConnection.Create(
                    connectionSettings,
                    eventStoreUri,
                    CONNECTION_NAME))
                {
                    await connection.ConnectAsync();
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
