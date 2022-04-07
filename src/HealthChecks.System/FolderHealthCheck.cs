using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.System
{
    public class FolderHealthCheck : IHealthCheck
    {
        private readonly FolderHealthCheckOptions _folderOptions;

        public FolderHealthCheck(FolderHealthCheckOptions folderOptions)
        {
            _folderOptions = folderOptions ?? throw new ArgumentNullException(nameof(folderOptions));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                List<string>? errorList = null;
                if (!string.IsNullOrEmpty(_folderOptions.Folder))
                {
                    if (!Directory.Exists(_folderOptions.Folder))
                    {
                        (errorList ??= new()).Add($"Folder {_folderOptions.Folder} does not exist.");
                    }
                }

                if (errorList?.Count > 0)
                {
                    return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: string.Join(" + ", errorList)));
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
