using HealthChecks.System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SystemHealthCheckBuilderExtensions
    {
        const string DISK_NAME = "diskstorage";
        const string MEMORY_NAME = "privatememory";
        const string WORKINGSET_NAME = "workingset";
        const string VIRTUALMEMORYSIZE_NAME = "virtualmemory";
        /// <summary>
        /// Add a health check for disk storage.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action method to configure the health check parameters.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'diskstorage' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddDiskStorageHealthCheck(this IHealthChecksBuilder builder, Action<DiskStorageOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            var options = new DiskStorageOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? DISK_NAME,
                sp => new DiskStorageHealthCheck(options),
                failureStatus,
                tags));
        }
        /// <summary>
        /// Add a health check for process private memory.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="maximumMemoryBytes">The maximum private memory bytes on the process.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'privatememory' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddPrivateMemoryHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? MEMORY_NAME,
                sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().PrivateMemorySize64),
                failureStatus,
                tags));
        }
        /// <summary>
        /// Add a health check for process working set.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="maximumMemoryBytes">The maximum working set memory bytes on the process.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'workingset' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddWorkingSetHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? WORKINGSET_NAME,
                sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().WorkingSet64),
                failureStatus,
                tags));
        }
        /// <summary>
        /// Add a health check for process virtual memory.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="maximumMemoryBytes">The maximum virtual memory bytes on the process.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'virtualmemory' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddVirtualMemorySizeHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? VIRTUALMEMORYSIZE_NAME,
                sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().VirtualMemorySize64),
                failureStatus,
                tags));
        }
    }
}
