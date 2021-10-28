using Dapr.Client;
using HealthChecks.Dapr;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
  public static class DaprHealthCheckBuilderExtensions
  {
    private const string Name = "dapr";

    public static IHealthChecksBuilder AddDapr(this IHealthChecksBuilder builder, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
    {
      return builder.Add(new HealthCheckRegistration(
         name ?? Name,
         sp => new DaprHealthCheck(sp.GetRequiredService<DaprClient>()),
         failureStatus,
         tags,
         timeout));
    }
  }
}
