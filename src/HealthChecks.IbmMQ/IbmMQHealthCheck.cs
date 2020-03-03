using IBM.WMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.IbmMQ
{
  public class IbmMQHealthCheck : IHealthCheck
  {
    private readonly Hashtable _connectionProperties;
    private readonly string _queueManager;

    public IbmMQHealthCheck(string queueManager, Hashtable connectionProperties)
    {
      _queueManager = queueManager;
      _connectionProperties = connectionProperties;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
      try
      {
        using (var connection = new MQQueueManager(_queueManager, _connectionProperties))
        {
          return Task.FromResult(
              HealthCheckResult.Healthy());
        }
      }
      catch (Exception ex)
      {
        return Task.FromResult(
            new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
      }
    }
  }
}
