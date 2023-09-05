using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.System;

public class FileHealthCheck : IHealthCheck
{
    private readonly FileHealthCheckOptions _fileOptions;

    public FileHealthCheck(FileHealthCheckOptions fileOptions)
    {
        _fileOptions = fileOptions;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> errorList = new();
            foreach (string folder in _fileOptions.Files)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    if (!File.Exists(folder))
                    {
                        errorList.Add($"File {folder} does not exist.");
                        if (!_fileOptions.CheckAllFiles)
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
