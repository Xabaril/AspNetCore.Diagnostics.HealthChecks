using HealthChecks.System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string DISK_NAME = "diskstorage";
        const string MEMORY_NAME = "privatememory";
        const string WORKINGSET_NAME = "workingset";
        const string VIRTUALMEMORYSIZE_NAME = "virtualmemory";

        public static IHealthChecksBuilder AddDiskStorageHealthCheck(this IHealthChecksBuilder builder, Action<DiskStorageOptions> setup)
        {
            var options = new DiskStorageOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
              DISK_NAME,
              sp => new DiskStorageHealthCheck(options, sp.GetService<ILogger<DiskStorageHealthCheck>>()),
              null,
              new string[] { DISK_NAME }));
        }

        public static IHealthChecksBuilder AddPrivateMemoryHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes)
        {
            return builder.Add(new HealthCheckRegistration(
             MEMORY_NAME,
             sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().PrivateMemorySize64),
             null,
             new string[] { MEMORY_NAME }));
        }

        public static IHealthChecksBuilder AddWorkingSetHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes)
        {
            return builder.Add(new HealthCheckRegistration(
             WORKINGSET_NAME,
             sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().WorkingSet64),
             null,
             new string[] { WORKINGSET_NAME }));
        }

        public static IHealthChecksBuilder AddVirtualMemorySizeHealthCheck(this IHealthChecksBuilder builder, long maximumMemoryBytes)
        {
            return builder.Add(new HealthCheckRegistration(
            VIRTUALMEMORYSIZE_NAME,
            sp => new MaximumValueHealthCheck<long>(maximumMemoryBytes, () => Process.GetCurrentProcess().VirtualMemorySize64),
            null,
            new string[] { VIRTUALMEMORYSIZE_NAME }));
        }
    }
}
