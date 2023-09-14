using System.Runtime.CompilerServices;
using HealthChecks.CosmosDb;

[assembly: TypeForwardedTo(typeof(TableServiceHealthCheck))]
[assembly: TypeForwardedTo(typeof(TableServiceHealthCheckOptions))]
