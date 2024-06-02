using System.Collections.ObjectModel;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.EventStore;

public class EventStoreHealthCheck : IHealthCheck
{
    private const string CONNECTION_NAME = "AspNetCore HealthCheck Connection";
    private const int ELAPSED_DELAY_MILLISECONDS = 500;
    private const int RECONNECTION_LIMIT = 1;

    private readonly string _eventStoreConnection;
    private readonly string? _login;
    private readonly string? _password;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.name", nameof(EventStoreHealthCheck) },
                    { "health_check.task", "ready" },
                    { "db.system.name", "eventstore" },
                    { "network.transport", "tcp" }
    };

    public EventStoreHealthCheck(string eventStoreConnection, string? login, string? password)
    {
        _eventStoreConnection = Guard.ThrowIfNull(eventStoreConnection);
        _login = login;
        _password = password;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            ConnectionSettingsBuilder connectionSettings;

            if (string.IsNullOrEmpty(_login) || string.IsNullOrEmpty(_password))
            {
                connectionSettings = ConnectionSettings.Create()
                    .LimitReconnectionsTo(RECONNECTION_LIMIT)
                    .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(ELAPSED_DELAY_MILLISECONDS))
                    .SetCompatibilityMode("auto");
            }
            else
            {
                connectionSettings = ConnectionSettings.Create()
                    .LimitReconnectionsTo(RECONNECTION_LIMIT)
                    .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(ELAPSED_DELAY_MILLISECONDS))
                    .SetDefaultUserCredentials(new UserCredentials(_login, _password))
                    .SetCompatibilityMode("auto");
            }

            using (var connection = EventStoreConnection.Create(
                _eventStoreConnection,
                connectionSettings,
                CONNECTION_NAME))
            {
                var tcs = new TaskCompletionSource<HealthCheckResult>(TaskCreationOptions.RunContinuationsAsynchronously);

                //connected
                connection.Connected += (s, e) => tcs.TrySetResult(HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails)));

                //connection closed after configured amount of failed reconnections
                connection.Closed += (s, e) =>
                {
                    tcs.TrySetResult(new HealthCheckResult(
                        status: context.Registration.FailureStatus,
                        description: e.Reason,
                        data: new ReadOnlyDictionary<string, object>(checkDetails)));
                };

                //connection error
                connection.ErrorOccurred += (s, e) =>
                {
                    tcs.TrySetResult(new HealthCheckResult(
                        status: context.Registration.FailureStatus,
                        exception: e.Exception,
                        data: new ReadOnlyDictionary<string, object>(checkDetails)));
                };

                using (cancellationToken.Register(() => connection.Close()))
                {
                    //completes after tcp connection init, but before successful connection and login
                    await connection.ConnectAsync().ConfigureAwait(false);
                }

                cancellationToken.ThrowIfCancellationRequested();

                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
