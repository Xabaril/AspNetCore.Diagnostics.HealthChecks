using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SmbCifs.DependencyInjection
{
    public static class SmbCifsHealthCheckStorageBuilderExtensions
    {

        private const string NAME = "smbcifs";

        /// <summary>
        /// Add a health check for Smb/Cifs (SharCifs used for Samba /Cifs Connections).
        /// Project uses the excellent port found at http://sharpcifsstd.dobes.jp/
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the Smb SmbCifs with basic parameters.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'smbcifs' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddSmbCifsBasicAuth(
            this IHealthChecksBuilder builder,
            Action<SmbCifsBasicOptions> setup,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = default)
        {

            var options = new SmbCifsBasicOptions();
            setup?.Invoke(options);

            return builder.Add(
                new HealthCheckRegistration(name ?? NAME,
                    sp => new SmbCifsStorageHealthCheck(options),
                    failureStatus,
                    tags,
                    timeout));
        }

        /// <summary>
        /// Add a health check for Smb/Cifs (SharCifs used for Samba /Cifs Connections).
        /// Project uses the excellent port found at http://sharpcifsstd.dobes.jp/
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the Smb SmbCifs with extend parameters.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'smbcifs' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddSmbCifsExtendedAuth(
            this IHealthChecksBuilder builder,
            Action<SmbCifsExtendedOptions> setup,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = default)
        {

            var options = new SmbCifsExtendedOptions();
            setup?.Invoke(options);

            return builder.Add(
                new HealthCheckRegistration(name ?? NAME,
                    sp => new SmbCifsStorageHealthCheck(options),
                    failureStatus,
                    tags,
                    timeout));
        }
    }
}
