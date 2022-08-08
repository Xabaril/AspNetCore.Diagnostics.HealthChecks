using System.Collections;
using IBM.WMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.IbmMQ;

public class IbmMQHealthCheck : IHealthCheck
{
    private readonly Hashtable _connectionProperties;
    private readonly string _queueManager;

    public IbmMQHealthCheck(string queueManager, Hashtable connectionProperties)
    {
        if (string.IsNullOrEmpty(queueManager))
            throw new ArgumentNullException(nameof(queueManager));

        _queueManager = queueManager;
        _connectionProperties = connectionProperties ?? throw new ArgumentNullException(nameof(connectionProperties));
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new MQQueueManager(_queueManager, _connectionProperties);
            return HealthCheckResultTask.Healthy;
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }
}
