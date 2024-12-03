using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using HealthChecks.System;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class SystemHealthCheckBuilderExtensions
{
    private const string DISK_NAME = "diskstorage";
    private const string MEMORY_NAME = "privatememory";
    private const string WORKINGSET_NAME = "workingset";
    private const string VIRTUALMEMORYSIZE_NAME = "virtualmemory";
    private const string PROCESS_NAME = "process";
    private const string PROCESS_ALLOCATED_MEMORY = "process_allocated_memory";
    private const string WINDOWS_SERVICE_NAME = "windowsservice";
    private const string FOLDER_NAME = "folder";
    private const string FILE_NAME = "file";

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
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDiskStorageHealthCheck(
        this IHealthChecksBuilder builder,
        Action<DiskStorageOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
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
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPrivateMemoryHealthCheck(
        this IHealthChecksBuilder builder,
        long maximumMemoryBytes,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
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
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddWorkingSetHealthCheck(
        this IHealthChecksBuilder builder,
        long maximumMemoryBytes,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
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
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddVirtualMemorySizeHealthCheck(
        this IHealthChecksBuilder builder,
        long maximumMemoryBytes,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
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
    /// <param name="processName">The name of the process.</param>
    /// <param name="predicate">Process[] predicate to configure checks.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'process' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddProcessHealthCheck(
        this IHealthChecksBuilder builder,
        string processName,
        Func<Process[], bool> predicate,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(processName, true);
        Guard.ThrowIfNull(predicate);

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
    /// <param name="maximumMegabytesAllocated">The maximum megabytes allowed to be allocated by the process.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'process' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddProcessAllocatedMemoryHealthCheck(
        this IHealthChecksBuilder builder,
        int maximumMegabytesAllocated,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        if (maximumMegabytesAllocated <= 0)
            throw new ArgumentException($"{nameof(maximumMegabytesAllocated)} should be greater than zero");

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
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="predicate">Process[] predicate to configure checks.</param>
    /// <param name="machineName">Machine where the service resides in. Optional.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'windowsservice' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
#if NET6_0_OR_GREATER
    [global::System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public static IHealthChecksBuilder AddWindowsServiceHealthCheck(
        this IHealthChecksBuilder builder,
        string serviceName,
        Func<ServiceController, bool> predicate,
        string? machineName = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException($"{nameof(WindowsServiceHealthCheck)} can only be registered in Windows Systems");
        }

        Guard.ThrowIfNull(serviceName, true);

        Guard.ThrowIfNull(predicate);

        return builder.Add(new HealthCheckRegistration(
            name ?? WINDOWS_SERVICE_NAME,
            sp => new WindowsServiceHealthCheck(serviceName, predicate, machineName),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a healthcheck that allows to check for the existence of one or more folders.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">Delegate for configuring the health check. Optional.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'folder' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddFolder(
        this IHealthChecksBuilder builder,
        Action<FolderHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new FolderHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? FOLDER_NAME,
            sp => new FolderHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a healthcheck that allows to check for the existence of one or more files.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">Delegate for configuring the health check. Optional.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'file' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddFile(
        this IHealthChecksBuilder builder,
        Action<FileHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return AddFile(builder, (_, options) => setup?.Invoke(options), name, failureStatus, tags, timeout);
    }

    /// <summary>
    /// Add a healthcheck that allows to check for the existence of one or more files.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action method to configure the health check parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'file' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddFile(
        this IHealthChecksBuilder builder,
        Action<IServiceProvider, FileHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? FILE_NAME,
            sp =>
            {
                var options = new FileHealthCheckOptions();
                setup?.Invoke(sp, options);
                return new FileHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }
}
