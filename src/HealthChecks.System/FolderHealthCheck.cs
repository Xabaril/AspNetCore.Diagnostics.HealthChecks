using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.System;

public class FolderHealthCheck : IHealthCheck
{
    private readonly FolderHealthCheckOptions _folderOptions;

    /// <summary>
    /// Creates an instance of <see cref="FolderHealthCheck"/> with the specified options.
    /// </summary>
    public FolderHealthCheck(FolderHealthCheckOptions folderOptions)
    {
        _folderOptions = Guard.ThrowIfNull(folderOptions);
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string>? errorList = null;
            foreach (string folder in _folderOptions.Folders)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    if (!Directory.Exists(folder))
                    {
                        (errorList ??= new()).Add($"Folder {folder} does not exist.");
                        if (!_folderOptions.CheckAllFolders)
                        {
                            break;
                        }
                    }
                }
            }

            return Task.FromResult(errorList.GetHealthState(context));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }
}
