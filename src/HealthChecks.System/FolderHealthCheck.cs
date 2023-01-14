using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.System
{
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
                List<string>? errorList = null;
                foreach (string folder in _folderOptions.Folders)
                {
                    if (!string.IsNullOrEmpty(folder))
                    {
                        if (!Directory.Exists(folder))
                        {
                            (errorList ??= new()).Add($"Folder {folder} does not exist.");
                        }
                    }
                }

                if (errorList?.Count > 0)
                {
                    return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: string.Join("; ", errorList)));
                }

                return HealthCheckResultTask.Healthy;
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
