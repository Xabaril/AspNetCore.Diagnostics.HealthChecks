using HealthChecks.System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SystemHealthCheckBuilderExtensions
    {
        const string DISK_NAME = "diskstorage";
        const string MEMORY_NAME = "privatememory";
        const string WORKINGSET_NAME = "workingset";
        const string VIRTUALMEMORYSIZE_NAME = "virtualmemory";
        const string PROCESS_NAME = "process";
        const string PROCESS_ALLOCATED_MEMORY = "process_allocated_memory";
        const string WINDOWS_SERVICE_NAME = "windowsservice";

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
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddDiskStorageHealthCheck(this IHealthChecksBuilder builder, Action<DiskStorageOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var options = new DiskStorageOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name ?? DISK_NAME,
                sp => new DiskStorageHealthCheck(options),
                failureStatus,
                tags,
                timeout));
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
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddPrivateMemoryHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? MEMORY_NAME,
                sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().PrivateMemorySize64),
                failureStatus,
                tags,
                timeout));
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
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddWorkingSetHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? WORKINGSET_NAME,
                sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().WorkingSet64),
                failureStatus,
                tags,
                timeout));
        }
        /// <summary>
        /// Add a health check to process virtual memory.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="maximumMemoryBytes">The maximum virtual memory bytes on the process.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'virtualmemory' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddVirtualMemorySizeHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? VIRTUALMEMORYSIZE_NAME,
                sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().VirtualMemorySize64),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a healthcheck that allows to check a predicate against the configured process name.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="processName">The name of the process</param>        
        /// <param name="predicate">Process[] predicate to configure checks</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'process' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddProcessHealthCheck(
            this IHealthChecksBuilder builder, string processName, Func<Process[], bool> predicate, string name = default,
            HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            if (string.IsNullOrEmpty(processName)) throw new ArgumentNullException(nameof(processName));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            return builder.Add(new HealthCheckRegistration(
                name ?? PROCESS_NAME,
                sp => new ProcessHealthCheck(processName, predicate),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Adds a healthcheck that allows to check the allocated bytes in memory and configure a threshold
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="maximumMegabytesAllocated">The maximum megabytes allowed to be allocated by the process</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'process' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddProcessAllocatedMemoryHealthCheck(
            this IHealthChecksBuilder builder, int maximumMegabytesAllocated, string name = default,
            HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            if (maximumMegabytesAllocated <= 0) throw new ArgumentException($"{nameof(maximumMegabytesAllocated)} should be greater than zero");

            return builder.Add(new HealthCheckRegistration(
                name ?? PROCESS_ALLOCATED_MEMORY,
                sp => new ProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated),
                failureStatus,
                tags,
                timeout));
        }


        /// <summary>
        /// Add a healthcheck that allows to check a predicate against the configured windows service.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="serviceName">The name of the service</param>
        /// <param name="predicate">Process[] predicate to configure checks</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'windowsservice' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddWindowsServiceHealthCheck(
            this IHealthChecksBuilder builder, string serviceName, Func<ServiceController, bool> predicate, string name = default,
            HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException($"{nameof(WindowsServiceHealthCheck)} can only be registered in Windows Systems");
            }

            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException(nameof(serviceName));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            return builder.Add(new HealthCheckRegistration(
                name ?? WINDOWS_SERVICE_NAME,
                sp => new WindowsServiceHealthCheck(serviceName, predicate),
                failureStatus,
                tags,
                timeout));
        }
    }

}