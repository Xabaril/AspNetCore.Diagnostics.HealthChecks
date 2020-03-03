using HealthChecks.IbmMQ;
using IBM.WMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Microsoft.Extensions.DependencyInjection
{
  public static class IbmMQHealthCheckBuilderExtensions
  {
    const string NAME = "ibmmq";

    /// <summary>
    /// Add a health check for IbmMQ services using connection properties.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="queueManager">The name of the queue manager to use.</param>
    /// <param name="connectionProperties">The list of properties that will be used for connection.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ibmmq' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
    public static IHealthChecksBuilder AddIbmMQ(this IHealthChecksBuilder builder, string queueManager, Hashtable connectionProperties, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
    {
      return builder.Add(new HealthCheckRegistration(
          name ?? NAME,
          new IbmMQHealthCheck(queueManager, connectionProperties),
          failureStatus,
          tags,
          timeout));
    }

    /// <summary>
    /// Add a health check for IbmMQ services using a managed connection.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="queueManager">The name of the queue manager to use.</param>
    /// <param name="channel">The name of the channel.</param>
    /// <param name="connectionInfo">The connection information in the following format HOSTNAME(PORT).</param>
    /// <param name="userName">The user name. Optional.</param>
    /// <param name="password">The password. Optional</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ibmmq' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
    public static IHealthChecksBuilder AddIbmMQManagedConnection(this IHealthChecksBuilder builder, string queueManager, string channel, string connectionInfo, string userName = null, string password = null, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
    {
      return builder.Add(new HealthCheckRegistration(
          name ?? NAME,
          new IbmMQHealthCheck(queueManager, BuildProperties(channel, connectionInfo, userName, password)),
          failureStatus,
          tags,
          timeout));
    }

    private static Hashtable BuildProperties(string channel, string connectionInfo, string userName = null, string password = null)
    {
      Hashtable properties = new Hashtable {
        {MQC.CHANNEL_PROPERTY, channel},
        {MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED},
        {MQC.CONNECTION_NAME_PROPERTY, connectionInfo}
      };

      if (!string.IsNullOrEmpty(userName))
        properties.Add(MQC.USER_ID_PROPERTY, userName);
      if (!string.IsNullOrEmpty(password))
        properties.Add(MQC.PASSWORD_PROPERTY, password);

      return properties;
    }
  }
}
