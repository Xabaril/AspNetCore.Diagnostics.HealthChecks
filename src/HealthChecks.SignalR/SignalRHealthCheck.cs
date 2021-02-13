using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.SignalR
{
    public class SignalRHealthCheck
        : IHealthCheck
    {
        private readonly Func<HubConnection> _hubConnectionBuilder;
        public SignalRHealthCheck(Func<HubConnection> hubConnectionBuilder)
        {
            _hubConnectionBuilder = hubConnectionBuilder ?? throw new ArgumentNullException(nameof(hubConnectionBuilder));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            HubConnection connection = null;

            try
            {
                connection = _hubConnectionBuilder();

                await connection.StartAsync(cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
            finally
            {
                await connection?.DisposeAsync().AsTask();
            }
        }
    }
}
