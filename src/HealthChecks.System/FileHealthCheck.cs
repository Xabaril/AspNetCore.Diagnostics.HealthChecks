using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.System;

public class FileHealthCheck : IHealthCheck
{
    private readonly FileHealthCheckOptions _fileOptions;

    /// <summary>
    /// Creates an instance of <see cref="FileHealthCheck"/> with the specified options.
    /// </summary>
    public FileHealthCheck(FileHealthCheckOptions fileOptions)
    {
        _fileOptions = fileOptions;
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> errorList = new();
            foreach (string file in _fileOptions.Files)
            {
                if (!string.IsNullOrEmpty(file))
                {
                    if (!File.Exists(file))
                    {
                        errorList.Add($"File {file} does not exist.");
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
