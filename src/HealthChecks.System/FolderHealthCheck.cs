using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.System;

public class FolderHealthCheck : IHealthCheck
{
    private readonly FolderHealthCheckOptions _folderOptions;

    public FolderHealthCheck(FolderHealthCheckOptions folderOptions)
    {
        _folderOptions = Guard.ThrowIfNull(folderOptions);
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            HealthCheckErrorList? errorList = null;
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

            return errorList?.GetHealthStateAsync(context) ?? HealthCheckResultTask.Healthy;
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }
}
