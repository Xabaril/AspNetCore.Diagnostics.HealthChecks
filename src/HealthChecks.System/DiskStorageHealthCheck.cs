using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.System
{
    public class DiskStorageHealthCheck
        : IHealthCheck
    {
        private readonly DiskStorageOptions _options;
        private readonly ILogger<DiskStorageHealthCheck> _logger;

        public DiskStorageHealthCheck(DiskStorageOptions options, ILogger<DiskStorageHealthCheck> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(DiskStorageHealthCheck)} is checking configured drives.");

                var configuredDrives = _options.ConfiguredDrives.Values;

                foreach (var item in configuredDrives)
                {
                    var systemDriveInfo = GetSystemDriveInfo(item.DriveName);

                    if (systemDriveInfo.Exists)
                    {
                        if (systemDriveInfo.ActualFreeMegabytes < item.MinimumFreeMegabytes)
                        {
                            _logger?.LogWarning($"The {nameof(DiskStorageHealthCheck)} check fail for drive {item.DriveName}.");

                            return Task.FromResult(
                                HealthCheckResult.Failed($"Minimum configured megabytes for disk {item.DriveName} is {item.MinimumFreeMegabytes} but actual free space are {systemDriveInfo.ActualFreeMegabytes} megabytes"));
                        }
                    }
                    else
                    {
                        _logger?.LogWarning($"{nameof(DiskStorageHealthCheck)} is checking a not present disk {item.DriveName} on system.");

                        return Task.FromResult(
                            HealthCheckResult.Failed($"Configured drive {item.DriveName} is not present on system"));
                    }
                }

                _logger?.LogDebug($"The {nameof(DiskStorageHealthCheck)} check success.");

                return Task.FromResult(
                    HealthCheckResult.Passed());
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(DiskStorageHealthCheck)} check fail with the exception {ex.ToString()}.");

                return Task.FromResult(
                    HealthCheckResult.Failed(exception:ex));
            }
        }

        private (bool Exists, long ActualFreeMegabytes) GetSystemDriveInfo(string driveName)
        {
            var driveInfo = DriveInfo.GetDrives()
                .FirstOrDefault(drive => String.Equals(drive.Name, driveName, StringComparison.InvariantCultureIgnoreCase));

            if (driveInfo != null)
            {
                return (true, driveInfo.AvailableFreeSpace / 1024 / 1024);
            }

            return (false, 0);
        }
    }
}
