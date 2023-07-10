namespace HealthChecks.System;

public class DiskStorageOptions
{
    internal Dictionary<string, (string DriveName, long MinimumFreeMegabytes)> ConfiguredDrives { get; } = new();

    public DiskStorageOptions AddDrive(string driveName, long minimumFreeMegabytes = 1)
    {
        ConfiguredDrives.Add(driveName, (driveName, minimumFreeMegabytes));
        return this;
    }

    public bool CheckAllDrives { get; set; }

    public DiskStorageOptions WithCheckAllDrives()
    {
        CheckAllDrives = true;
        return this;
    }

    /// <summary>
    /// Allows to set custom description of the failed disk check.
    /// </summary>
    public ErrorDescription FailedDescription = (driveName, minimumFreeMegabytes, actualFreeMegabytes)
        => actualFreeMegabytes == null
            ? $"Configured drive {driveName} is not present on system"
            : $"Minimum configured megabytes for disk {driveName} is {minimumFreeMegabytes} but actual free space are {actualFreeMegabytes} megabytes";

    public delegate string ErrorDescription(string driveName, long minimumFreeMegabytes, long? actualFreeMegabytes);
}
