using System.Collections.Generic;

namespace HealthChecks.System
{
    public class DiskStorageOptions
    {
        internal Dictionary<string, (string DriveName, long MinimumFreeMegabytes)> ConfiguredDrives { get; } = new Dictionary<string, (string DriveName, long MinimumFreeMegabytes)>();
        public DiskStorageOptions AddDrive(string driveName, long minimumFreeMegabytes = 1)
        {
            ConfiguredDrives.Add(driveName, (driveName, minimumFreeMegabytes));
            return this;
        }
    }
}
